# Epic 3: Chat data model and migrations

Дата: 2026-04-23  
Статус: implemented

## ER-модель (chat-domain)

- `Chat` 1 -> N `Message`
- `Message` 1 -> N `MessageImage`
- `Chat` 1 -> N `ProcessingJob`
- `Message` 1 -> N `ProcessingJob`
- `User` 1 -> N `Chat`

## Сущности и поля

### `Chat` (`app.chats`)

- required: `id`, `user_id`, `status`, `created_at`
- optional: `title`, `updated_at`, `last_message_at`, audit soft-delete поля
- статус хранится строкой enum (`Idle`, `Processing`, `Error`)

### `Message` (`app.messages`)

- required: `id`, `chat_id`, `role`, `status`, `retry_count`, `created_at`
- optional: `text_html`, `failure_code`, `failure_reason`, `client_request_id`, audit soft-delete поля
- `client_request_id` nullable + unique index с filter `IS NOT NULL`
- `role`: `User`, `Assistant`, `System`
- `status`: `Queued`, `Processing`, `Completed`, `Failed`

### `MessageImage` (`app.message_images`)

- required: `id`, `message_id`, `storage_url`, `mime_type`, `size_bytes`, `sort_order`, `created_at`
- optional: `width`, `height`, `plant_id_raw_json`, `plant_id_normalized_json`, audit soft-delete поля
- `plant_id_raw_json` и `plant_id_normalized_json` хранятся как `jsonb`

### `ProcessingJob` (`app.processing_jobs`)

- required: `id`, `chat_id`, `message_id`, `status`, `attempt`, `max_attempts`, `created_at`
- optional: `locked_until`, `last_error`, audit soft-delete поля
- `status`: `Queued`, `Processing`, `Completed`, `Failed`

## Стратегия хранения изображений

- в БД хранится `storage_url` + метаданные (`mime_type`, `size_bytes`, `width`, `height`, `sort_order`);
- бинарные данные файлов в БД не сохраняются;
- результаты PlantId сохраняются в `jsonb` полях на уровне `MessageImage`.

## Связи и ограничения целостности

- `Chat.user_id -> User.id` (`Restrict`)
- `Message.chat_id -> Chat.id` (`Cascade`)
- `MessageImage.message_id -> Message.id` (`Cascade`)
- `ProcessingJob.chat_id -> Chat.id` (`Cascade`)
- `ProcessingJob.message_id -> Message.id` (`Cascade`)

## Индексы

- `Chat(user_id, updated_at desc)` для истории
- `Message(chat_id, created_at)` для ленты
- `ProcessingJob(status, created_at)` для worker
- `Message(client_request_id)` unique when not null

## Миграции и команды

Создать миграцию:

```powershell
dotnet ef migrations add AddChatDomainModel --project Infrastructure/Infrastructure.csproj --startup-project Api/Api.csproj --context AppDbContext --output-dir Db/App/Migrations
```

Применить миграции:

```powershell
dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project Api/Api.csproj --context AppDbContext
```

Проверить список миграций:

```powershell
dotnet ef migrations list --project Infrastructure/Infrastructure.csproj --startup-project Api/Api.csproj --context AppDbContext
```
