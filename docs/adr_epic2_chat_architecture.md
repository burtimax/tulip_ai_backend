# ADR Epic 2: Chat processing architecture

Дата: 2026-04-23  
Статус: accepted

## Контекст

Для перехода к реализации Chat MVP требуется зафиксировать:

- ответственность слоев `Api` / `Application` / `Infrastructure` / `Shared`;
- место размещения worker-а;
- модель очереди и порядок обработки сообщений.

## Решение

### 1) Ответственность слоев

- `Api`: HTTP endpoints, middleware, DI composition root, hosted services.
- `Application`: orchestration/use-cases чата, pipeline обработки сообщений, вызовы внешних сервисов через абстракции.
- `Infrastructure`: EF Core (`AppDbContext`), репозитории/persistence, миграции.
- `Shared`: контракты DTO/ошибок, конфигурация, утилиты и общие модели.

### 2) Размещение worker-а

На этапе MVP worker размещается как hosted service в процессе `Api`:

- упрощает деплой (один процесс);
- упрощает транзакционную постановку в очередь и обработку;
- достаточно для целевой нагрузки MVP.

Вынесение в отдельный процесс допускается после стабилизации метрик и нагрузки.

### 3) Модель очереди и порядок обработки

Принята DB-backed очередь на основе `ProcessingJob`:

- producer: API создает `Message` + `ProcessingJob(status=Queued)`;
- consumer: hosted worker периодически выбирает `Queued` jobs батчами;
- lock: optimistic/lease подход по `lockedUntil` и статусу `Processing`;
- retries: ограниченное число попыток `MaxAttempts`, затем `Failed`.

Порядок обработки:

1. FIFO по `ProcessingJob.CreatedAt`;
2. внутри сообщения изображения обрабатываются строго последовательно по `MessageImage.SortOrder`;
3. переходы статусов канонические: `Queued -> Processing -> Completed/Failed`.

## Последствия

- Плюсы: минимальная сложность MVP, быстрый запуск, прозрачная трассировка.
- Риски: вертикальное масштабирование ограничено рамками одного API-процесса.
- Митигация: заложены конфиги очереди (`ChatQueue`) и корреляция логов для будущего выноса worker-а.
