using System.Text.Json;
using Infrastructure.Db.App;
using Infrastructure.Db.App.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services.BootstrapDatabase;

/// <summary>
/// Заполняет базу данных начальными данными при старте приложения.
/// </summary>
public class DatabaseBootstrap : IDatabaseBootstrap
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,

    };

    private readonly AppDbContext _db;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<DatabaseBootstrap> _logger;

    public DatabaseBootstrap(
        AppDbContext db,
        IHostEnvironment hostEnvironment,
        ILogger<DatabaseBootstrap> logger)
    {
        _db = db;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await SeedKnowledgeBaseAsync(cancellationToken);
    }

    private async Task SeedKnowledgeBaseAsync(CancellationToken cancellationToken)
    {
        var filePath = Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, "..", "docs", "knowledge_base.json"));
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Файл базы знаний не найден: {FilePath}", filePath);
            return;
        }

        List<KnowledgeCategorySeedDto>? categories;
        try
        {
            string json = await File.ReadAllTextAsync(filePath, cancellationToken);
            categories = JsonSerializer.Deserialize<List<KnowledgeCategorySeedDto>>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Ошибка десериализации базы знаний из файла: {FilePath}", filePath);
            return;
        }

        if (categories is null || categories.Count == 0)
        {
            _logger.LogInformation("Файл базы знаний пустой: {FilePath}", filePath);
            return;
        }

        var categoryIds = categories.Select(x => x.Id).ToHashSet();
        var existingCategoryIds = await _db.KnowledgeCategories
            .AsNoTracking()
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var existingCategoryIdSet = existingCategoryIds.ToHashSet();

        var missingCategories = categories
            .Where(x => existingCategoryIdSet.Contains(x.Id) == false)
            .Select(x => new KnowledgeCategoryEntity
            {
                Id = x.Id,
                Name = x.Name,
                Order = x.Order
            })
            .ToList();
        if (missingCategories.Count > 0)
            _db.KnowledgeCategories.AddRange(missingCategories);

        var existingCategories = await _db.KnowledgeCategories
            .Where(x => categoryIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        foreach (var existingCategory in existingCategories)
        {
            var seedCategory = categories.First(x => x.Id == existingCategory.Id);
            if (existingCategory.Order != seedCategory.Order)
                existingCategory.Order = seedCategory.Order;
        }

        var postIds = categories
            .SelectMany(x => x.Posts)
            .Select(x => x.Id)
            .ToHashSet();
        var existingPostIds = await _db.KnowledgeTitles
            .AsNoTracking()
            .Where(x => postIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var existingPostIdSet = existingPostIds.ToHashSet();

        var missingTitles = new List<KnowledgeTitleEntity>();
        foreach (var category in categories)
        {
            foreach (var post in category.Posts.Where(post => existingPostIdSet.Contains(post.Id) == false))
            {
                missingTitles.Add(new KnowledgeTitleEntity
                {
                    Id = post.Id,
                    KnowledgeCategoryId = category.Id,
                    Title = post.Title,
                    Order = post.Order,
                    Excerpt = post.Excerpt,
                    Images = post.Images,
                    Content = post.Content
                });
            }
        }

        if (missingTitles.Count > 0)
            _db.KnowledgeTitles.AddRange(missingTitles);

        var existingTitles = await _db.KnowledgeTitles
            .Where(x => postIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
        foreach (var existingTitle in existingTitles)
        {
            var seedTitle = categories
                .SelectMany(x => x.Posts)
                .First(x => x.Id == existingTitle.Id);
            if (existingTitle.Order != seedTitle.Order)
                existingTitle.Order = seedTitle.Order;
        }

        if (missingCategories.Count == 0 && missingTitles.Count == 0)
            return;

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "База знаний обновлена: добавлено категорий {CategoryCount}, статей {TitleCount}",
            missingCategories.Count,
            missingTitles.Count);
    }

    private sealed class KnowledgeCategorySeedDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<KnowledgeTitleSeedDto> Posts { get; set; } = [];
    }

    private sealed class KnowledgeTitleSeedDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? Excerpt { get; set; }
        public List<string> Images { get; set; } = [];
        public string Content { get; set; } = string.Empty;
    }

}
