# Epic 7 Runbook: reliability and operations

Дата: 2026-04-23  
Статус: active

## Что реализовано

- Retry/backoff для внешних интеграций уже выполняется в `PlantIdService` и `OpenRouterService`.
- Ограничение числа попыток обработки job:
  - `ProcessingJob.MaxAttempts`
  - после превышения job переводится в `Failed`.
- Для восстановления добавлен replay endpoint:
  - `POST /chats/jobs/{jobId}/replay` (переводит `Failed -> Queued`).
- Добавлен rate-limit на отправку сообщений:
  - middleware `ChatMessageRateLimitMiddleware`
  - ограничение: `20` запросов в минуту на ключ `ip + userId`.

## Метрики пайплайна

Регистрируются через `System.Diagnostics.Metrics` (`TulipAI.ChatPipeline`):

- `chat_queue_wait_seconds`
- `chat_processing_seconds`
- `chat_plantid_latency_seconds`
- `chat_llm_latency_seconds`
- `chat_jobs_processed`
- `chat_jobs_failed`

## Логи и корреляция

- На уровне API: `CorrelationId`, `ChatId`, `MessageId`, `JobId`.
- На уровне worker: scope-логи с `ChatId/MessageId/JobId`.
- Ошибки обработки пишутся с кодом (`external_plantid_error`/`external_llm_error`) и `last_error`.

## Рекомендованные алерты (MVP)

- `chat_jobs_failed / (chat_jobs_processed + chat_jobs_failed) > 0.1` за 10 мин.
- `p95(chat_processing_seconds) > 45s` за 15 мин.
- `p95(chat_plantid_latency_seconds) > 15s` за 15 мин.
- `p95(chat_llm_latency_seconds) > 20s` за 15 мин.
- резкий рост HTTP `429` на `POST /chats/{chatId}/messages`.

## Процедура восстановления зависших/failed job

1. Найти `processing_jobs` со статусом `Failed` и `last_error`.
2. Проверить доступность PlantId/OpenRouter и лимиты.
3. Для повторной обработки вызвать:
   - `POST /chats/jobs/{jobId}/replay`
4. Убедиться, что job перешёл в `Queued`, затем `Processing/Completed`.

## Процедура при деградации внешних сервисов

1. Проверить таймауты и долю ошибок в метриках `chat_plantid_latency_seconds`, `chat_llm_latency_seconds`.
2. При массовой деградации временно ограничить входящий поток (rate-limit).
3. Зафиксировать инцидент: временной интервал, affected chats, коды ошибок.
4. После восстановления — выполнить replay `Failed` задач.
