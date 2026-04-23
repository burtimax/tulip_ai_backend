using Infrastructure.Db.App;
using Infrastructure.Db.App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Chat;

public sealed class ChatService : IChatService
{
    private readonly AppDbContext _dbContext;

    public ChatService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ChatEntity> CreateChatAsync(Guid userId, string? title, CancellationToken cancellationToken = default)
    {
        var chat = new ChatEntity
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Title = title?.Trim(),
            Status = ChatStatus.Idle
        };

        _dbContext.Chats.Add(chat);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return chat;
    }

    public async Task<IReadOnlyList<ChatEntity>> GetChatsAsync(Guid userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Chats
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<ChatEntity?> GetChatAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chats
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == chatId, cancellationToken);
    }

    public async Task<IReadOnlyList<MessageEntity>> GetMessagesAsync(Guid chatId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Messages
            .AsNoTracking()
            .Include(x => x.Images.OrderBy(i => i.SortOrder))
            .Where(x => x.ChatId == chatId)
            .OrderBy(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<MessageEntity> EnqueueMessageAsync(
        Guid chatId,
        string? textHtml,
        IReadOnlyList<ChatIncomingImage> incomingImages,
        string? clientRequestId,
        CancellationToken cancellationToken = default)
    {
        var message = new MessageEntity
        {
            Id = Guid.CreateVersion7(),
            ChatId = chatId,
            Role = MessageRole.User,
            Status = MessageStatus.Queued,
            TextHtml = textHtml,
            ClientRequestId = string.IsNullOrWhiteSpace(clientRequestId) ? null : clientRequestId.Trim()
        };

        _dbContext.Messages.Add(message);

        var index = 0;
        foreach (var image in incomingImages)
        {
            _dbContext.MessageImages.Add(new MessageImageEntity
            {
                Id = Guid.CreateVersion7(),
                MessageId = message.Id,
                StorageUrl = image.DataUrl,
                MimeType = image.MimeType,
                SizeBytes = image.SizeBytes,
                Width = image.Width,
                Height = image.Height,
                SortOrder = index++
            });
        }

        _dbContext.ProcessingJobs.Add(new ProcessingJobEntity
        {
            Id = Guid.CreateVersion7(),
            ChatId = chatId,
            MessageId = message.Id,
            Status = JobStatus.Queued
        });

        var chat = await _dbContext.Chats.FirstOrDefaultAsync(x => x.Id == chatId, cancellationToken);
        if (chat is null)
            throw new InvalidOperationException($"Chat {chatId} not found");

        chat.Status = ChatStatus.Processing;
        chat.LastMessageAt = DateTimeOffset.UtcNow;
        chat.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return message;
    }
}
