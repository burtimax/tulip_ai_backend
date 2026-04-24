using System.Text.Json;
using System.Diagnostics;
using Infrastructure.Db.App;
using Infrastructure.Db.App.Entities;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModuleLLM.Configuration;
using ModuleLLM.Models.OpenRouter;
using ModuleLLM.Services;
using ModulePlantId.Models;
using ModulePlantId.Services;
using Shared.Configs;

namespace Application.Services.Chat;

public sealed class ChatProcessingService : IChatProcessingService
{
    private const int MaxChatHistoryMessages = 20;
    private readonly AppDbContext _dbContext;
    private readonly IPlantIdService _plantIdService;
    private readonly ILlmApiService _llmApiService;
    private readonly ChatQueueConfiguration _queueOptions;
    private readonly OpenRouterApiConfiguration _openRouterConfig;
    private readonly ILogger<ChatProcessingService> _logger;
    private readonly ChatProcessingMetrics _metrics;

    public ChatProcessingService(
        AppDbContext dbContext,
        IPlantIdService plantIdService,
        ILlmApiService llmApiService,
        IOptions<ChatQueueConfiguration> queueOptions,
        OpenRouterApiConfiguration openRouterConfig,
        ChatProcessingMetrics metrics,
        ILogger<ChatProcessingService> logger)
    {
        _dbContext = dbContext;
        _plantIdService = plantIdService;
        _llmApiService = llmApiService;
        _queueOptions = queueOptions.Value;
        _openRouterConfig = openRouterConfig;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task<int> ProcessQueuedJobsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var jobs = await _dbContext.ProcessingJobs
            .Include(x => x.Message)
            .ThenInclude(x => x.Images.OrderBy(i => i.SortOrder))
            .Include(x => x.Chat)
            .Where(x => x.Status == JobStatus.Queued && (x.LockedUntil == null || x.LockedUntil < now))
            .OrderBy(x => x.CreatedAt)
            .Take(_queueOptions.BatchSize)
            .ToListAsync(cancellationToken);

        if (jobs.Count == 0)
            return 0;

        foreach (var job in jobs)
        {
            _metrics.RecordQueueWait(DateTimeOffset.UtcNow - job.CreatedAt);
            await ProcessSingleJobAsync(job, cancellationToken);
        }

        return jobs.Count;
    }

    private async Task ProcessSingleJobAsync(ProcessingJobEntity job, CancellationToken cancellationToken)
    {
        if (job.Status is JobStatus.Completed or JobStatus.Failed)
            return;

        var now = DateTimeOffset.UtcNow;
        var processingSw = Stopwatch.StartNew();
        job.Status = JobStatus.Processing;
        job.LockedUntil = now.AddSeconds(_queueOptions.LockTimeoutSeconds);
        job.Attempt += 1;
        job.Message.Status = MessageStatus.Processing;
        job.Chat.Status = ChatStatus.Processing;
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            using var logScope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["ChatId"] = job.ChatId,
                ["MessageId"] = job.MessageId,
                ["JobId"] = job.Id
            });

            var (plantContext, plantImageUrls) = await BuildPlantContextAsync(job.Message, cancellationToken);
            var request = BuildRequest(plantContext, plantImageUrls, job.Message.TextHtml);
            var chatHistory = await LoadChatHistoryForLlmAsync(job.ChatId, job.MessageId, cancellationToken);

            var llmRequest = new OpenRouterChatRequest
            {
                Model = _openRouterConfig.Model,
                Messages = new List<OpenRouterMessage>()
            };
            llmRequest.Messages.Add(new OpenRouterMessage
            {
                Role = "system",
                Content = PromptBuilder.MainPrompt,
            });
            llmRequest.Messages.AddRange(chatHistory);
            llmRequest.Messages.Add(new OpenRouterMessage { Role = "user", Content = request });

            var llmSw = Stopwatch.StartNew();
            var llmResult = await _llmApiService.SendChatCompletionAsync(llmRequest, cancellationToken);
            llmSw.Stop();
            _metrics.RecordLlmLatency(llmSw.Elapsed);
            if (!llmResult.IsSuccess || llmResult.Value?.Choices.FirstOrDefault()?.Message?.Content is not { } reply || string.IsNullOrWhiteSpace(reply))
            {
                throw new InvalidOperationException(llmResult.Error ?? "LLM returned empty response");
            }

            var newMessage = new MessageEntity
            {
                Id = Guid.CreateVersion7(),
                ChatId = job.ChatId,
                Role = MessageRole.Assistant,
                Status = MessageStatus.Completed,
                TextHtml = reply.Trim(),
            };

            if (plantImageUrls is not null && plantImageUrls.Any())
            {
                int c = 0;
                var provider = new FileExtensionContentTypeProvider();
                newMessage.Images = plantImageUrls.Select(i => new MessageImageEntity() {
                        StorageUrl = i,
                        SortOrder = ++c,
                        MimeType = provider.TryGetContentType(i.Split('/').LastOrDefault(), out var mimeType) ? mimeType : "application/octet-stream" })
                    .ToList();
            }

            _dbContext.Messages.Add(newMessage);

            job.Message.Status = MessageStatus.Completed;
            job.Message.FailureCode = null;
            job.Message.FailureReason = null;
            job.Status = JobStatus.Completed;
            job.LastError = null;
            _metrics.MarkProcessed();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chat job failed. JobId={JobId}, MessageId={MessageId}", job.Id, job.MessageId);
            job.LastError = ex.Message;
            job.Message.FailureCode = ex.Message.Contains("Plant.id", StringComparison.OrdinalIgnoreCase)
                ? "external_plantid_error"
                : "external_llm_error";
            job.Message.FailureReason = ex.Message;

            if (job.Attempt >= Math.Max(1, job.MaxAttempts))
            {
                job.Status = JobStatus.Failed;
                job.Message.Status = MessageStatus.Failed;
                _metrics.MarkFailed();
            }
            else
            {
                job.Status = JobStatus.Queued;
                job.Message.Status = MessageStatus.Queued;
            }
        }

        await UpdateChatAggregateStatusAsync(job.ChatId, cancellationToken);
        processingSw.Stop();
        _metrics.RecordProcessingTime(processingSw.Elapsed);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<List<OpenRouterMessage>> LoadChatHistoryForLlmAsync(
        Guid chatId,
        Guid currentMessageId,
        CancellationToken cancellationToken)
    {
        var history = await _dbContext.Messages
            .Where(x =>
                x.ChatId == chatId &&
                x.Id != currentMessageId &&
                x.Status == MessageStatus.Completed &&
                !string.IsNullOrWhiteSpace(x.TextHtml))
            .OrderByDescending(x => x.CreatedAt)
            .Take(MaxChatHistoryMessages)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new { x.Role, x.TextHtml })
            .ToListAsync(cancellationToken);

        return history
            .Select(x => new OpenRouterMessage
            {
                Role = MapMessageRoleToLlmRole(x.Role),
                Content = x.TextHtml!
            })
            .ToList();
    }

    private static string MapMessageRoleToLlmRole(MessageRole role) =>
        role switch
        {
            MessageRole.User => "user",
            MessageRole.Assistant => "assistant",
            MessageRole.System => "system",
            _ => "user"
        };

    private async Task<(string ContextText, List<string> ImageUrls)> BuildPlantContextAsync(
        MessageEntity message,
        CancellationToken cancellationToken)
    {
        if (message.Images.Count == 0)
            return ("Изображения не приложены.", new List<string>());

        var perImage = new List<object>();
        var contextParts = new List<string>();
        var imageUrls = new List<string>();

        foreach (var image in message.Images.OrderBy(x => x.SortOrder))
        {
            var analyzeRequest = new PlantIdAnalyzeRequest
            {
                Images = new List<string> { image.StorageUrl },
                Health = "all",
                SimilarImages = true
            };

            var plantIdSw = Stopwatch.StartNew();
            var result = await _plantIdService.CreateIdentificationAsync(analyzeRequest, cancellationToken: cancellationToken);
            plantIdSw.Stop();
            _metrics.RecordPlantIdLatency(plantIdSw.Elapsed);
            if (!result.IsSuccess || result.Value is null)
                throw new InvalidOperationException($"Plant.id error: {result.Error ?? "unknown"}");

            image.PlantIdRawJson = string.IsNullOrWhiteSpace(result.Value.RawJson)
                ? JsonSerializer.Serialize(result.Value)
                : result.Value.RawJson;

            var plantResult = result.Value.Result;
            var isPlant = plantResult?.IsPlant?.Binary == true;
            var isPlantProbabilityPercent = ToPercent(plantResult?.IsPlant?.Probability);

            string textBlock;
            string? plantImageUrl = null;
            string? diseaseImageUrl = null;
            string? plantName = null;
            double? plantProbability = null;
            bool? isHealthy = null;
            string? diseaseName = null;
            double? diseaseProbability = null;

            var attachedImageUrls = new List<string>();

            if (!isPlant)
            {
                textBlock =
                    $"Фото {image.SortOrder + 1}: На фото, скорее всего, не растение. Вероятность, что это растение: {isPlantProbabilityPercent}.";
            }
            else
            {
                var topPlant = plantResult?.Classification?.Suggestions
                    ?.OrderByDescending(x => x.Probability ?? 0)
                    .FirstOrDefault();
                plantName = topPlant?.Name;
                plantProbability = topPlant?.Probability;
                plantImageUrl = topPlant?.SimilarImages.FirstOrDefault()?.Url;
                isHealthy = plantResult?.IsHealthy?.Binary;

                var blockLines = new List<string>
                {
                    $"Фото {image.SortOrder + 1}: На фото определено растение: {plantName ?? "не удалось определить"}.",
                    $"Вероятность определения растения: {ToPercent(plantProbability)}."
                };

                if (isHealthy == true)
                {
                    blockLines.Add("Болезни у растения отсутствуют.");
                }
                else
                {
                    var topDisease = plantResult?.Disease?.Suggestions
                        ?.OrderByDescending(x => x.Probability ?? 0)
                        .FirstOrDefault();
                    diseaseName = topDisease?.Name;
                    diseaseProbability = topDisease?.Probability;
                    diseaseImageUrl = topDisease?.SimilarImages.FirstOrDefault()?.Url;

                    blockLines.Add($"Возможное заболевание: {diseaseName ?? "не удалось определить"}.");
                    blockLines.Add($"Вероятность заболевания: {ToPercent(diseaseProbability)}.");
                }

                if (!string.IsNullOrWhiteSpace(plantImageUrl))
                {
                    imageUrls.Add(plantImageUrl);
                    attachedImageUrls.Add(plantImageUrl);
                    blockLines.Add($"Прикрепил [Фото {imageUrls.Count}]: Наиболее похожее фото растения.");
                }

                if (!string.IsNullOrWhiteSpace(diseaseImageUrl))
                {
                    imageUrls.Add(diseaseImageUrl);
                    attachedImageUrls.Add(diseaseImageUrl);
                    blockLines.Add($"Прикрепил [Фото {imageUrls.Count}]: Наиболее похожее фото заболевания.");
                }

                textBlock = string.Join(Environment.NewLine, blockLines);
            }

            contextParts.Add(textBlock);

            var normalized = new
            {
                isPlant = plantResult?.IsPlant?.Binary,
                isPlantProbability = plantResult?.IsPlant?.Probability,
                isHealthy = plantResult?.IsHealthy?.Binary,
                topPlant = plantName is null ? null : new { Name = plantName, Probability = plantProbability, ImageUrl = plantImageUrl },
                topDisease = diseaseName is null ? null : new { Name = diseaseName, Probability = diseaseProbability, ImageUrl = diseaseImageUrl },
                attachedImageUrls
            };
            image.PlantIdNormalizedJson = JsonSerializer.Serialize(normalized);
            perImage.Add(new { image.SortOrder, normalized });
        }

        var contextText = string.Join($"{Environment.NewLine}{Environment.NewLine}", contextParts);
        return (contextText, imageUrls.Distinct(StringComparer.OrdinalIgnoreCase).ToList());
    }

    private static string BuildRequest(string plantContext, IReadOnlyCollection<string> plantImageUrls, string? userTextHtml)
    {
        var userText = string.IsNullOrWhiteSpace(userTextHtml)
            ? "Пользовательский вопрос отсутствует, дай краткую диагностическую рекомендацию по фото."
            : userTextHtml;
        var imageAttachments = plantImageUrls.Count == 0
            ? "Дополнительные похожие фото отсутствуют."
            : string.Join(
                Environment.NewLine,
                plantImageUrls.Select((url, index) => $"Фото {index + 1}: {url}"));

        return $"""
            Контекст PlantId по изображениям (структурированный текст):
            {plantContext}

            Список похожих фото, которые нужно учитывать как прикреплённые:
            {imageAttachments}

            Вопрос от пользователя:
            {userText}
            """;
    }

    private static string ToPercent(double? value)
    {
        if (value is null)
            return "неизвестно";

        return $"{Math.Round(value.Value * 100, 2):0.##}%";
    }

    private async Task UpdateChatAggregateStatusAsync(Guid chatId, CancellationToken cancellationToken)
    {
        var chat = await _dbContext.Chats.FirstAsync(x => x.Id == chatId, cancellationToken);
        var hasProcessing = await _dbContext.Messages.AnyAsync(
            x => x.ChatId == chatId && (x.Status == MessageStatus.Queued || x.Status == MessageStatus.Processing),
            cancellationToken);
        var hasErrors = await _dbContext.Messages.AnyAsync(
            x => x.ChatId == chatId && x.Status == MessageStatus.Failed,
            cancellationToken);

        chat.Status = hasProcessing ? ChatStatus.Processing : hasErrors ? ChatStatus.Error : ChatStatus.Idle;
        chat.UpdatedAt = DateTimeOffset.UtcNow;
        chat.LastMessageAt = await _dbContext.Messages
            .Where(x => x.ChatId == chatId)
            .MaxAsync(x => (DateTimeOffset?)x.CreatedAt, cancellationToken);
    }
}
