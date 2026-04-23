using Infrastructure.Db.App;
using Infrastructure.Db.App.Entities;
using Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Configs;

namespace Application.Services.Chat;

public sealed class ChatService : IChatService
{
    private readonly AppDbContext _dbContext;
    private readonly ChatQueueConfiguration _queueOptions;

    public ChatService(AppDbContext dbContext, IOptions<ChatQueueConfiguration> queueOptions)
    {
        _dbContext = dbContext;
        _queueOptions = queueOptions.Value;
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

    public async Task<PagedList<ChatEntity>> GetChatsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Chats
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt);

        return await PagedList<ChatEntity>.ToPagedListAsync(query, pageNumber, pageSize);
    }

    public Task<ChatEntity?> GetChatAsync(Guid chatId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chats
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == chatId, cancellationToken);
    }

    public async Task<PagedList<MessageEntity>> GetMessagesAsync(
        Guid chatId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Messages
            .AsNoTracking()
            .Include(x => x.Images.OrderBy(i => i.SortOrder))
            .Where(x => x.ChatId == chatId)
            .OrderBy(x => x.CreatedAt);

        return await PagedList<MessageEntity>.ToPagedListAsync(query, pageNumber, pageSize);
    }

    public Task<MessageEntity?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Messages
            .AsNoTracking()
            .Include(x => x.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == messageId, cancellationToken);
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
            Status = JobStatus.Queued,
            Attempt = 0,
            MaxAttempts = _queueOptions.MaxAttempts
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

    public async Task<bool> ReplayFailedJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.ProcessingJobs
            .Include(x => x.Message)
            .Include(x => x.Chat)
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);
        if (job is null || job.Status != JobStatus.Failed)
            return false;

        job.Status = JobStatus.Queued;
        job.Attempt = 0;
        job.LastError = null;
        job.LockedUntil = null;
        job.Message.Status = MessageStatus.Queued;
        job.Message.FailureCode = null;
        job.Message.FailureReason = null;
        job.Chat.Status = ChatStatus.Processing;
        job.Chat.UpdatedAt = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
