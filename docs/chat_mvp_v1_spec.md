# Chat MVP v1 Specification

Дата: 2026-04-23
Статус: approved-for-implementation
Зависимости: Plant.id v3, OpenRouterService, PostgreSQL

## 1. Scope Chat-only MVP

### 1.1 Границы MVP

В рамках Chat MVP реализуются:

- создание чата;
- получение списка чатов пользователя;
- получение чата по id;
- отправка сообщения в чат;
- получение сообщений чата;
- асинхронная обработка сообщения через очередь job-ов.

Вне рамок Chat MVP:

- streaming токенов LLM;
- realtime-доставка через WebSocket/SSE (допустим polling);
- редактирование/удаление сообщений;
- вложения, кроме изображений;
- синхронный ответ LLM в запросе отправки сообщения.

### 1.2 Поддерживаемые типы входящих сообщений

- text-only: сообщение содержит только текст (HTML после sanitization);
- image-only: сообщение содержит только изображения;
- mixed: сообщение содержит и текст, и изображения.

Валидация минимального содержания:

- сообщение считается валидным, если содержит непустой `textHtml` или как минимум 1 изображение;
- сообщение, где отсутствует и текст, и изображения, отклоняется как validation error.

### 1.3 Минимальные сценарии

- success: обработка успешно завершена, создан ответ `Assistant`;
- plantid-error: ошибка Plant.id переводит исходное сообщение в `Failed` с user-friendly описанием;
- llm-error: ошибка LLM переводит исходное сообщение в `Failed` с user-friendly описанием;
- retry: допускается автоматическая повторная попытка для transient-ошибок (ограниченное число попыток).

## 2. Контракты доменных сущностей

### 2.1 Chat

- `id: Guid`
- `userId: Guid`
- `title: string?`
- `status: ChatStatus` (`Idle`, `Processing`, `Error`)
- `createdAt: DateTimeOffset`
- `updatedAt: DateTimeOffset`
- `lastMessageAt: DateTimeOffset?`

### 2.2 Message

- `id: Guid`
- `chatId: Guid`
- `role: MessageRole` (`User`, `Assistant`, `System`)
- `textHtml: string?`
- `status: MessageStatus` (`Queued`, `Processing`, `Completed`, `Failed`)
- `failureCode: string?`
- `failureReason: string?`
- `retryCount: int`
- `createdAt: DateTimeOffset`
- `updatedAt: DateTimeOffset`

### 2.3 MessageImage

- `id: Guid`
- `messageId: Guid`
- `storageUrl: string`
- `mimeType: string`
- `sizeBytes: long`
- `width: int?`
- `height: int?`
- `sortOrder: int`
- `plantIdRawJson: string?`
- `plantIdNormalizedJson: string?`
- `createdAt: DateTimeOffset`

### 2.4 ProcessingJob

- `id: Guid`
- `chatId: Guid`
- `messageId: Guid`
- `status: JobStatus` (`Queued`, `Processing`, `Completed`, `Failed`)
- `attempt: int`
- `maxAttempts: int`
- `lockedUntil: DateTimeOffset?`
- `lastError: string?`
- `createdAt: DateTimeOffset`
- `updatedAt: DateTimeOffset`

## 3. Статус-машины

### 3.1 Message status machine

Канонический переход:

`Queued -> Processing -> Completed`
`Queued -> Processing -> Failed`

Ограничения:

- переход из `Completed` и `Failed` в иные статусы запрещен;
- повторная обработка допускается только пока сообщение не терминальное;
- `retryCount` увеличивается перед каждой новой попыткой.

### 3.2 Chat aggregated status

- `Idle`: активных сообщений/задач в `Queued/Processing` нет, ошибок нет;
- `Processing`: есть хотя бы одно сообщение или job в `Queued/Processing`;
- `Error`: нет активной обработки, но есть последнее пользовательское сообщение в `Failed`.

## 4. Канонический алгоритм обработки

### 4.1 Обработка изображений

1. Worker забирает `ProcessingJob` в статусе `Queued`.
2. Переводит `ProcessingJob` и связанный `Message` в `Processing`.
3. Если у сообщения есть изображения, каждое изображение обрабатывается строго последовательно по `sortOrder`.
4. По каждому изображению вызывается Plant.id endpoint `create identification`.

### 4.2 Выбор top-гипотез

Для каждого изображения:

- растение: брать top-1 из `result.classification.suggestions` по `probability`;
- болезни: брать top-3 из `result.disease.suggestions` по `probability`;
- если блок `disease` отсутствует, считать "данные о болезни не определены".

### 4.3 Агрегация контекста

На уровне сообщения формируется агрегированный контекст:

- список top-гипотез растений по изображениям;
- список top-гипотез заболеваний по изображениям;
- признаки `is_plant` и `is_healthy`;
- confidence-маркеры для осторожных формулировок в ответе.

### 4.4 Формирование prompt

Prompt собирается из трех частей:

1. системные инструкции по домену тюльпанов;
2. агрегированный Plant.id контекст;
3. пользовательский текст (если передан).

Если пользовательский текст отсутствует, LLM получает явный маркер "вопрос не задан, требуется диагностический ответ по фото".

### 4.5 Финализация

- при успехе создается `Assistant` message, исходное сообщение переводится в `Completed`;
- при ошибке исходное сообщение переводится в `Failed`, фиксируются `failureCode` и `failureReason`;
- статус чата пересчитывается в конце каждого job;
- клиент получает результат через read-endpoint (polling).

## 5. API v1 контракты

## 5.1 Endpoints чатов

- `POST /chats`
- `GET /chats`
- `GET /chats/{chatId}`

## 5.2 Endpoints сообщений

- `POST /chats/{chatId}/messages`
- `GET /chats/{chatId}/messages`

## 5.3 Контракт `POST /chats/{chatId}/messages`

Успешный ответ (`202 Accepted`):

```json
{
  "messageId": "7dd10488-f962-4f99-96f2-3f2cf2e1fb9a",
  "status": "Queued",
  "createdAt": "2026-04-23T18:42:10.493Z"
}
```

## 5.4 Формат ошибок и валидации

Единый формат:

```json
{
  "error": {
    "code": "validation_error",
    "message": "Message must contain text or at least one image",
    "details": [
      {
        "field": "textHtml",
        "issue": "empty_with_no_images"
      }
    ],
    "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00"
  }
}
```

Коды ошибок MVP:

- `validation_error`
- `not_found`
- `conflict`
- `external_plantid_error`
- `external_llm_error`
- `internal_error`

## 6. Нефункциональные условия для Epic 1

- ответы API и данные БД в UTC;
- корреляция логов по `chatId`, `messageId`, `jobId`;
- html на входе проходит sanitization whitelist-подходом;
- изображения ограничиваются допустимыми MIME: `image/jpeg`, `image/png`, `image/webp`.

## 7. Definition of Done (Epic 1)

Epic 1 считается завершенным, когда:

- scope Chat MVP зафиксирован;
- утверждены контракты сущностей и статусов;
- зафиксирован канонический pipeline обработки;
- утверждены API v1 endpoints и error contract;
- спецификация признана baseline для реализации Epic 2+.
