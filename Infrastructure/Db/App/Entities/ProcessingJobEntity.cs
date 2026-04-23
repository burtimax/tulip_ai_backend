using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Db.App.Entities;

/// <summary>
/// Задача фоновой обработки сообщения.
/// </summary>
public sealed class ProcessingJobEntity : BaseEntity
{
    [Comment("Идентификатор чата")]
    public Guid ChatId { get; set; }

    public ChatEntity Chat { get; set; } = null!;

    [Comment("Идентификатор сообщения")]
    public Guid MessageId { get; set; }

    public MessageEntity Message { get; set; } = null!;

    [Comment("Статус задачи")]
    public JobStatus Status { get; set; } = JobStatus.Queued;

    [Comment("Номер попытки")]
    public int Attempt { get; set; } = 1;

    [Comment("Максимальное число попыток")]
    public int MaxAttempts { get; set; } = 3;

    [Comment("Время удержания lock на задаче")]
    public DateTimeOffset? LockedUntil { get; set; }

    [Comment("Последняя ошибка обработки")]
    public string? LastError { get; set; }
}
