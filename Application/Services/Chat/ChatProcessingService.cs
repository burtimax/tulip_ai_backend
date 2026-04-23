using System.Text.Json;
using Infrastructure.Db.App;
using Infrastructure.Db.App.Entities;
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
    private readonly AppDbContext _dbContext;
    private readonly IPlantIdService _plantIdService;
    private readonly ILlmApiService _llmApiService;
    private readonly ChatQueueConfiguration _queueOptions;
    private readonly OpenRouterApiConfiguration _openRouterConfig;
    private readonly ILogger<ChatProcessingService> _logger;

    public ChatProcessingService(
        AppDbContext dbContext,
        IPlantIdService plantIdService,
        ILlmApiService llmApiService,
        IOptions<ChatQueueConfiguration> queueOptions,
        OpenRouterApiConfiguration openRouterConfig,
        ILogger<ChatProcessingService> logger)
    {
        _dbContext = dbContext;
        _plantIdService = plantIdService;
        _llmApiService = llmApiService;
        _queueOptions = queueOptions.Value;
        _openRouterConfig = openRouterConfig;
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
            await ProcessSingleJobAsync(job, cancellationToken);
        }

        return jobs.Count;
    }

    private async Task ProcessSingleJobAsync(ProcessingJobEntity job, CancellationToken cancellationToken)
    {
        if (job.Status is JobStatus.Completed or JobStatus.Failed)
            return;

        var now = DateTimeOffset.UtcNow;
        job.Status = JobStatus.Processing;
        job.LockedUntil = now.AddSeconds(_queueOptions.LockTimeoutSeconds);
        job.Attempt += 1;
        job.Message.Status = MessageStatus.Processing;
        job.Chat.Status = ChatStatus.Processing;
        await _dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var plantContext = await BuildPlantContextAsync(job.Message, cancellationToken);
            var prompt = BuildPrompt(plantContext, job.Message.TextHtml);

            var llmRequest = new OpenRouterChatRequest
            {
                Model = _openRouterConfig.Model,
                Messages = new List<OpenRouterMessage>
                {
                    new() { Role = "system", Content = "Ты агрономический AI-ассистент по тюльпанам. Отвечай конкретно и на русском языке." },
                    new() { Role = "user", Content = prompt }
                }
            };

            var llmResult = await _llmApiService.SendChatCompletionAsync(llmRequest, cancellationToken);
            if (!llmResult.IsSuccess || llmResult.Value?.Choices.FirstOrDefault()?.Message?.Content is not { } reply || string.IsNullOrWhiteSpace(reply))
            {
                throw new InvalidOperationException(llmResult.Error ?? "LLM returned empty response");
            }

            _dbContext.Messages.Add(new MessageEntity
            {
                Id = Guid.CreateVersion7(),
                ChatId = job.ChatId,
                Role = MessageRole.Assistant,
                Status = MessageStatus.Completed,
                TextHtml = reply.Trim()
            });

            job.Message.Status = MessageStatus.Completed;
            job.Message.FailureCode = null;
            job.Message.FailureReason = null;
            job.Status = JobStatus.Completed;
            job.LastError = null;
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
            }
            else
            {
                job.Status = JobStatus.Queued;
                job.Message.Status = MessageStatus.Queued;
            }
        }

        await UpdateChatAggregateStatusAsync(job.ChatId, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> BuildPlantContextAsync(MessageEntity message, CancellationToken cancellationToken)
    {
        if (message.Images.Count == 0)
            return "Изображения не приложены.";

        var perImage = new List<object>();
        foreach (var image in message.Images.OrderBy(x => x.SortOrder))
        {
            var analyzeRequest = new PlantIdAnalyzeRequest
            {
                Images = new List<string> { image.StorageUrl },
                Health = "all",
                SimilarImages = true
            };

            var result = await _plantIdService.CreateIdentificationAsync(analyzeRequest, cancellationToken: cancellationToken);
            if (!result.IsSuccess || result.Value is null)
                throw new InvalidOperationException($"Plant.id error: {result.Error ?? "unknown"}");

            image.PlantIdRawJson = string.IsNullOrWhiteSpace(result.Value.RawJson)
                ? JsonSerializer.Serialize(result.Value)
                : result.Value.RawJson;

            var topPlant = result.Value.Result?.Classification?.Suggestions
                ?.OrderByDescending(x => x.Probability ?? 0)
                .FirstOrDefault();
            var topDiseases = result.Value.Result?.Disease?.Suggestions?
                .OrderByDescending(x => x.Probability ?? 0)
                .Take(3)
                .Select(x => (object)new { x.Name, x.Probability })
                .ToList() ?? new List<object>();

            var normalized = new
            {
                isPlant = result.Value.Result?.IsPlant?.Binary,
                isHealthy = result.Value.Result?.IsHealthy?.Binary,
                topPlant = topPlant is null ? null : new { topPlant.Name, topPlant.Probability },
                topDiseases
            };
            image.PlantIdNormalizedJson = JsonSerializer.Serialize(normalized);
            perImage.Add(new { image.SortOrder, normalized });
        }

        return JsonSerializer.Serialize(perImage);
    }

    private static string BuildPrompt(string plantContextJson, string? userTextHtml)
    {
        var userText = string.IsNullOrWhiteSpace(userTextHtml)
            ? "Пользовательский вопрос отсутствует, дай диагностическую рекомендацию по фото."
            : userTextHtml;

        return $"""
            Контекст PlantId по изображениям:
            {plantContextJson}

            Текст пользователя:
            {userText}

            Сформируй краткий практический ответ по уходу за тюльпанами, укажи вероятные проблемы и шаги.
            """;
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
