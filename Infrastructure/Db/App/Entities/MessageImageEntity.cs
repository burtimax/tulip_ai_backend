using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Db.App.Entities;

/// <summary>
/// Изображение, привязанное к сообщению.
/// </summary>
public sealed class MessageImageEntity : BaseEntity
{
    [Comment("Идентификатор сообщения")]
    public Guid MessageId { get; set; }

    [JsonIgnore]
    public MessageEntity Message { get; set; } = null!;

    [Comment("URL изображения в хранилище")]
    public string StorageUrl { get; set; } = string.Empty;

    [Comment("MIME-тип изображения")]
    public string MimeType { get; set; } = string.Empty;

    [Comment("Размер изображения в байтах")]
    public long SizeBytes { get; set; }

    [Comment("Ширина изображения")]
    public int? Width { get; set; }

    [Comment("Высота изображения")]
    public int? Height { get; set; }

    [Comment("Порядок изображения в сообщении")]
    public int SortOrder { get; set; }

    [JsonIgnore]
    [Comment("Сырой ответ PlantId")]
    public string? PlantIdRawJson { get; set; }

    [JsonIgnore]
    [Comment("Нормализованный ответ PlantId")]
    public string? PlantIdNormalizedJson { get; set; }
}
