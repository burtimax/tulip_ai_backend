using Infrastructure.Db.App.Entities;
using Infrastructure.Models;

namespace Application.Services.Chat;

public interface IChatService
{
    Task<ChatEntity> CreateChatAsync(Guid userId, string? title, CancellationToken cancellationToken = default);
    Task<PagedList<ChatEntity>> GetChatsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ChatEntity?> GetChatAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task<PagedList<MessageEntity>> GetMessagesAsync(Guid chatId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<MessageEntity?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task<MessageEntity> EnqueueMessageAsync(
        Guid chatId,
        string? textHtml,
        IReadOnlyList<ChatIncomingImage> incomingImages,
        string? clientRequestId,
        CancellationToken cancellationToken = default);
    Task<bool> ReplayFailedJobAsync(Guid jobId, CancellationToken cancellationToken = default);
}

public sealed class ChatIncomingImage
{
    public required string DataUrl { get; init; }
    public required string MimeType { get; init; }
    public required long SizeBytes { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
}
