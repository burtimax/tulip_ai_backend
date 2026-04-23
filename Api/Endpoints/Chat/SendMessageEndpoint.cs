using Application.Services.Chat;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Shared.Configs;
using Shared.Contracts;

namespace Api.Endpoints.Chat;

public sealed class SendMessageEndpoint : Endpoint<SendMessageRequest, Result<SendMessageResponse>>
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
        AllowFileUploads();
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
        var images = req.Images ?? new List<IFormFile>();

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
            if (image.Length <= 0)
            {
                await ChatEndpointErrors.WriteValidationErrorAsync(
                    HttpContext,
                    "Each image must be non-empty",
                    cancellationToken: ct);
                return;
            }

            if (string.IsNullOrWhiteSpace(image.ContentType))
            {
                await ChatEndpointErrors.WriteValidationErrorAsync(
                    HttpContext,
                    "Each image must contain a mime type",
                    cancellationToken: ct);
                return;
            }

            if (!_storageOptions.AllowedMimeTypes.Contains(image.ContentType, StringComparer.OrdinalIgnoreCase))
            {
                await ChatEndpointErrors.WriteValidationErrorAsync(
                    HttpContext,
                    $"Unsupported mimeType: {image.ContentType}",
                    cancellationToken: ct);
                return;
            }

            var maxSizeBytes = _storageOptions.MaxImageSizeMb * 1024L * 1024L;
            if (image.Length > maxSizeBytes)
            {
                await ChatEndpointErrors.WriteValidationErrorAsync(
                    HttpContext,
                    $"Image size must be in range 1..{maxSizeBytes} bytes",
                    cancellationToken: ct);
                return;
            }

            await using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream, ct);
            var bytes = memoryStream.ToArray();
            var base64 = Convert.ToBase64String(bytes);
            var dataUrl = $"data:{image.ContentType};base64,{base64}";

            incomingImages.Add(new ChatIncomingImage
            {
                DataUrl = dataUrl,
                MimeType = image.ContentType.Trim().ToLowerInvariant(),
                SizeBytes = image.Length,
                Width = null,
                Height = null
            });
        }

        var message = await _chatService.EnqueueMessageAsync(
            chatId,
            sanitizedHtml,
            incomingImages,
            req.ClientRequestId,
            ct);

        await SendAsync(new Result<SendMessageResponse>(new SendMessageResponse
        {
            MessageId = message.Id,
            Status = message.Status.ToString(),
            CreatedAt = message.CreatedAt
        }), 202, ct);
    }
}
