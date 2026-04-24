using Infrastructure.Db.App;
using Infrastructure.Db.App.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Knowledge;

public sealed class KnowledgeService : IKnowledgeService
{
    private readonly AppDbContext _dbContext;

    public KnowledgeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<KnowledgeCategoryEntity>> GetKnowledgeBaseAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.KnowledgeCategories
            .AsNoTracking()
            .Include(x => x.Titles.OrderBy(t => t.Order).ThenBy(t => t.CreatedAt))
            .OrderBy(x => x.Order)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
