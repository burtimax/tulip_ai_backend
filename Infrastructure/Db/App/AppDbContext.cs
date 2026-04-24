using Infrastructure.Db.App.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Db.App;

public partial class AppDbContext : DbContext
{
    private const string appSchema = "app";
    private const string statSchema = "stat";
    private const string knowledgeSchema = "knowledge";

    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<StatEventEntity> StatEvents => Set<StatEventEntity>();
    public DbSet<LlmUsageEntity> LlmUsages => Set<LlmUsageEntity>();
    public DbSet<LogEntity> Logs => Set<LogEntity>();
    public DbSet<ChatEntity> Chats => Set<ChatEntity>();
    public DbSet<MessageEntity> Messages => Set<MessageEntity>();
    public DbSet<MessageImageEntity> MessageImages => Set<MessageImageEntity>();
    public DbSet<ProcessingJobEntity> ProcessingJobs => Set<ProcessingJobEntity>();
    public DbSet<KnowledgeCategoryEntity> KnowledgeCategories => Set<KnowledgeCategoryEntity>();
    public DbSet<KnowledgeTitleEntity> KnowledgeTitles => Set<KnowledgeTitleEntity>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        SetSchemasToTables(builder);
        SetAllToSnakeCase(builder);
        AppDbContext.ConfigureEntities(builder);
        AppDbContext.SetFilters(builder);
    }
}
