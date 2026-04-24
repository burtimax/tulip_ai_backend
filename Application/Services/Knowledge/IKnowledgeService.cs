using Infrastructure.Db.App.Entities;

namespace Application.Services.Knowledge;

public interface IKnowledgeService
{
    Task<IReadOnlyList<KnowledgeCategoryEntity>> GetKnowledgeBaseAsync(CancellationToken cancellationToken = default);
}
