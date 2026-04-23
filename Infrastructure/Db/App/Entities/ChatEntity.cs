using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Db.App.Entities;

/// <summary>
/// Чат пользователя.
/// </summary>
public sealed class ChatEntity : BaseEntity
{
    [Comment("Идентификатор пользователя")]
    public Guid UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    [Comment("Краткий заголовок чата")]
    public string? Title { get; set; }

    [Comment("Агрегированный статус чата")]
    public ChatStatus Status { get; set; } = ChatStatus.Idle;

    [Comment("Время последнего сообщения в чате")]
    public DateTimeOffset? LastMessageAt { get; set; }

    public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
    public ICollection<ProcessingJobEntity> ProcessingJobs { get; set; } = new List<ProcessingJobEntity>();
}
