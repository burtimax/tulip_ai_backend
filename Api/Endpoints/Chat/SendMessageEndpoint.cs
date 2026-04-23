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
        var chat = await _chatService.GetChatAsync(chatId, ct);
        if (chat is null)
        {
            await ChatEndpointErrors.WriteNotFoundAsync(HttpContext, "chat not found", ct);
            return;
        }

        var sanitizedHtml = ChatHtmlSanitizer.Sanitize(req.TextHtml);
        var images = req.Images ?? new List<IncomingImageDto>();

        if (string.IsNullOrWhiteSpace(sanitizedHtml) && images.Count == 0)
        {
            await ChatEndpointErrors.WriteValidationErrorAsync(
                HttpContext,
                "Message must contain textHtml or at least one image",
                cancellationToken: ct);
            return;
        }

        if (images.Count > _storageOptions.MaxImagesPerMessage)
        {
            await ChatEndpointErrors.WriteValidationErrorAsync(
                HttpContext,
                $"Images count must be <= {_storageOptions.MaxImagesPerMessage}",
                cancellationToken: ct);
            return;
        }

        var incomingImages = new List<ChatIncomingImage>();
        foreach (var image in images)
        {
            if (string.IsNullOrWhiteSpace(image.DataUrl) || string.IsNullOrWhiteSpace(image.MimeType))
            {
                await ChatEndpointErrors.WriteValidationErrorAsync(
                    HttpContext,
                    "Each image must contain dataUrl and mimeType",
                    cancellationToken: ct);
                return;
            }

            if (!_storageOptions.AllowedMimeTypes.Contains(image.MimeType, StringComparer.OrdinalIgnoreCase))
            {
                await ChatEndpointErrors.WriteValidationErrorAsync(
                    HttpContext,
                    $"Unsupported mimeType: {image.MimeType}",
                    cancellationToken: ct);
                return;
            }

            var maxSizeBytes = _storageOptions.MaxImageSizeMb * 1024L * 1024L;
            if (image.SizeBytes <= 0 || image.SizeBytes > maxSizeBytes)
            {
                await ChatEndpointErrors.WriteValidationErrorAsync(
                    HttpContext,
                    $"Image size must be in range 1..{maxSizeBytes} bytes",
                    cancellationToken: ct);
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
