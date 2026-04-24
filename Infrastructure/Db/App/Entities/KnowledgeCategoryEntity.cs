using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Db.App.Entities;

/// <summary>
/// Категория статей базы знаний.
/// </summary>
public sealed class KnowledgeCategoryEntity : BaseEntity
{
    [Comment("Название категории базы знаний")]
    public string Name { get; set; } = string.Empty;

    [Comment("Порядок сортировки категории")]
    public int Order { get; set; }

    public ICollection<KnowledgeTitleEntity> Titles { get; set; } = new List<KnowledgeTitleEntity>();
}
