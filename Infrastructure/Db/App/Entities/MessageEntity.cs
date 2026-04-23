using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Db.App.Entities;

/// <summary>
/// Сообщение в чате.
/// </summary>
public sealed class MessageEntity : BaseEntity
{
    [Comment("Идентификатор чата")]
    public Guid ChatId { get; set; }

    public ChatEntity Chat { get; set; } = null!;

    [Comment("Роль отправителя")]
    public MessageRole Role { get; set; } = MessageRole.User;

    [Comment("Санитизированный HTML пользователя или ассистента")]
    public string? TextHtml { get; set; }

    [Comment("Статус обработки сообщения")]
    public MessageStatus Status { get; set; } = MessageStatus.Queued;

    [Comment("Код ошибки обработки")]
    public string? FailureCode { get; set; }

    [Comment("Причина ошибки обработки")]
    public string? FailureReason { get; set; }

    [Comment("Число попыток обработки")]
    public int RetryCount { get; set; }

    [Comment("Ключ идемпотентности клиентского запроса")]
    public string? ClientRequestId { get; set; }

    public ICollection<MessageImageEntity> Images { get; set; } = new List<MessageImageEntity>();
    public ICollection<ProcessingJobEntity> ProcessingJobs { get; set; } = new List<ProcessingJobEntity>();
}
