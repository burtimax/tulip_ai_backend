Команды для миграции БД.
```
# Применять команду в папке проекта
dotnet ef migrations add RemoveConfirmedProp --context AppDbContext --project Infrastructure -o Db/App/Migrations
# Удаление последней миграции
dotnet ef migrations remove --context AppDbContext --project Infrastructure
# Применение миграции
dotnet ef database update Name --context AppDbContext --project Infrastructure
```
