using System.Linq.Expressions;
using Infrastructure.Db.App.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Extensions;

namespace Infrastructure.Db.App;

public partial class AppDbContext
{
    /// <summary>
    /// Таблицы, свойства, ключи, внеш. ключи, индексы переводит в нижний регистр в БД.
    /// </summary>
    protected void SetAllToSnakeCase(ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            entityType.SetTableName(entityType.GetTableName().ToSnakeCase());

            foreach (var property in entityType.GetProperties())
            {
                var schema = entityType.GetSchema();
                var tableName = entityType.GetTableName();
                var storeObjectIdentifier = StoreObjectIdentifier.Table(tableName, schema);
                property.SetColumnName(property.GetColumnName(storeObjectIdentifier).ToSnakeCase());
            }

            foreach (var key in entityType.GetKeys())
                key.SetName(key.GetName().ToSnakeCase());

            foreach (var key in entityType.GetForeignKeys())
                key.SetConstraintName(key.GetConstraintName().ToSnakeCase());

            foreach (var index in entityType.GetIndexes())
                index.SetDatabaseName(index.GetDatabaseName().ToSnakeCase());
        }
    }

    /// <summary>
    /// Задать наименование таблиц и схемы для таблиц.
    /// </summary>
    private void SetSchemasToTables(ModelBuilder builder)
    {
        // Identity таблицы
        builder.Entity<UserEntity>().ToTable("users", appSchema);

        // Прикладные таблицы
        builder.Entity<StatEventEntity>().ToTable("stat_events", statSchema);
        builder.Entity<LlmUsageEntity>().ToTable("llm_usages", statSchema);
    }

    /// <summary>
    /// Настройка фильтров запросов.
    /// </summary>
    public static void SetFilters(ModelBuilder modelBuilder)
    {
        // Фильтр для UserEntity (наследуется от IdentityUser, но реализует IBaseEntity)
        modelBuilder.Entity<UserEntity>()
            .HasQueryFilter(e => e.DeletedAt == null);

        // Фильтр для остальных сущностей, наследующих от BaseEntity
        var entities = modelBuilder.Model
            .GetEntityTypes()
            .Where(e => e.ClrType.BaseType == typeof(BaseEntity))
            .Select(e => e.ClrType);

        Expression<Func<BaseEntity, bool>>
            expression = del => del.DeletedAt == null;

        foreach (var e in entities)
        {
            ParameterExpression p = Expression.Parameter(e);
            Expression body =
                ReplacingExpressionVisitor
                    .Replace(expression.Parameters.Single(),
                        p, expression.Body);

            modelBuilder.Entity(e)
                .HasQueryFilter(
                    Expression.Lambda(body, p));
        }
    }

    public static void ConfigureEntities(ModelBuilder builder)
    {


        // Настройка связи StatEventEntity -> UserEntity
        builder.Entity<StatEventEntity>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.SetNull);


        builder.Entity<LogEntity>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<LogEntity>()
            .Property(l => l.Log)
            .HasColumnType("jsonb");

        builder.Entity<LlmUsageEntity>()
            .Property(e => e.InputJson)
            .HasColumnType("jsonb");
    }
}
