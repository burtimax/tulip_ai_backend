using Application.Services.Chat;
using FastEndpoints;
using Microsoft.Extensions.Options;
using Shared.Configs;

namespace Api.Endpoints.Chat;

public sealed class SendMessageEndpoint : Endpoint<SendMessageRequest, SendMessageResponse>
{
    private readonly IChatService _chatService;
    private readonly ChatStorageConfiguration _storageOptions;

    public SendMessageEndpoint(IChatService chatService, IOptions<ChatStorageConfiguration> storageOptions)
    {
        _chatService = chatService;
        _storageOptions = storageOptions.Value;
    }

    public override void Configure()
    {
        Post("/{chatId:guid}/messages");
        Group<ChatGroupEndpoints>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(SendMessageRequest req, CancellationToken ct)
    {
        var chatId = Route<Guid>("chatId");
        var sanitizedHtml = ChatHtmlSanitizer.Sanitize(req.TextHtml);
        var images = req.Images ?? new List<IncomingImageDto>();

        if (string.IsNullOrWhiteSpace(sanitizedHtml) && images.Count == 0)
        {
            await SendAsync(new SendMessageResponse
            {
                MessageId = Guid.Empty,
                Status = "validation_error",
                CreatedAt = DateTimeOffset.UtcNow
            }, 400, ct);
            return;
        }

        if (images.Count > _storageOptions.MaxImagesPerMessage)
        {
            await SendAsync(new SendMessageResponse
            {
                MessageId = Guid.Empty,
                Status = "validation_error",
                CreatedAt = DateTimeOffset.UtcNow
            }, 400, ct);
            return;
        }

        var incomingImages = new List<ChatIncomingImage>();
        foreach (var image in images)
        {
            if (string.IsNullOrWhiteSpace(image.DataUrl) || string.IsNullOrWhiteSpace(image.MimeType))
            {
                await SendAsync(new SendMessageResponse
                {
                    MessageId = Guid.Empty,
                    Status = "validation_error",
                    CreatedAt = DateTimeOffset.UtcNow
                }, 400, ct);
                return;
            }

            if (!_storageOptions.AllowedMimeTypes.Contains(image.MimeType, StringComparer.OrdinalIgnoreCase))
            {
                await SendAsync(new SendMessageResponse
                {
                    MessageId = Guid.Empty,
                    Status = "validation_error",
                    CreatedAt = DateTimeOffset.UtcNow
                }, 400, ct);
                return;
            }

            var maxSizeBytes = _storageOptions.MaxImageSizeMb * 1024L * 1024L;
            if (image.SizeBytes <= 0 || image.SizeBytes > maxSizeBytes)
            {
                await SendAsync(new SendMessageResponse
                {
                    MessageId = Guid.Empty,
                    Status = "validation_error",
                    CreatedAt = DateTimeOffset.UtcNow
                }, 400, ct);
                return;
            }

            incomingImages.Add(new ChatIncomingImage
            {
                DataUrl = image.DataUrl.Trim(),
                MimeType = image.MimeType.Trim().ToLowerInvariant(),
                SizeBytes = image.SizeBytes,
                Width = image.Width,
                Height = image.Height
            });
        }

        var message = await _chatService.EnqueueMessageAsync(
            chatId,
            sanitizedHtml,
            incomingImages,
            req.ClientRequestId,
            ct);

        await SendAsync(new SendMessageResponse
        {
            MessageId = message.Id,
            Status = message.Status.ToString(),
            CreatedAt = message.CreatedAt
        }, 202, ct);
    }
}
