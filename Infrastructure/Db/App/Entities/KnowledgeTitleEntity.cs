using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Db.App.Entities;

/// <summary>
/// Статья базы знаний.
/// </summary>
public sealed class KnowledgeTitleEntity : BaseEntity
{
    [Comment("Идентификатор категории")]
    public Guid KnowledgeCategoryId { get; set; }

    [JsonIgnore]
    public KnowledgeCategoryEntity Category { get; set; } = null!;

    [Comment("Заголовок статьи")]
    public string Title { get; set; } = string.Empty;

    [Comment("Порядок сортировки статьи внутри категории")]
    public int Order { get; set; }

    [Comment("Краткое описание статьи")]
    public string? Excerpt { get; set; }

    [Comment("Список изображений статьи")]
    public List<string> Images { get; set; } = [];

    [Comment("HTML-контент статьи")]
    public string Content { get; set; } = string.Empty;
}
