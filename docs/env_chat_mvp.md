# Chat MVP env/config variables

Документ фиксирует конфигурацию для Epic 2 и правило хранения секретов.

## Обязательные секции

- `Database:AppDbConnection`
- `OpenRouter:ApiKey`, `OpenRouter:BaseUrl`, `OpenRouter:Model`
- `PlantId:ApiKey`, `PlantId:BaseUrl`, `PlantId:AnalyzeEndpoint`
- `ChatQueue:*`
- `ChatStorage:*`
- `Proxy:*` (если используется)
- `S3:*` (если `ChatStorage:Provider = S3`)

## Пример переменных окружения (Windows)

```powershell
$env:Database__AppDbConnection="Host=localhost;Port=5432;Database=tulip;Username=postgres;Password=postgres"
$env:OpenRouter__ApiKey="..."
$env:OpenRouter__BaseUrl="https://openrouter.ai/api/v1"
$env:OpenRouter__Model="google/gemini-2.5-flash"
$env:PlantId__ApiKey="..."
$env:PlantId__BaseUrl="https://plant.id"
$env:PlantId__AnalyzeEndpoint="api/v3/identification"
$env:ChatQueue__PollIntervalSeconds="3"
$env:ChatQueue__LockTimeoutSeconds="120"
$env:ChatQueue__MaxAttempts="3"
$env:ChatQueue__BatchSize="10"
$env:ChatStorage__Provider="S3"
$env:ChatStorage__MaxImagesPerMessage="5"
$env:ChatStorage__MaxImageSizeMb="10"
$env:S3__ServiceUrl="https://s3.timeweb.cloud"
$env:S3__BucketName="..."
$env:S3__AccessKey="..."
$env:S3__SecretKey="..."
```

## Политика хранения секретов

- локально: `User Secrets` или переменные окружения;
- CI/CD: secret storage платформы (GitHub Actions Secrets / Vault / аналог);
- запрещено хранить production-ключи в `appsettings*.json`;
- для dev допускаются только placeholder-значения в репозитории.

## Fail-fast policy

На старте приложения выполняется валидация конфигурации (`ValidateOnStart`) для:

- `ProcessorJob`
- `ChatQueue`
- `ChatStorage`
- `OpenRouter`
- `PlantId`
- `Proxy`

При невалидной конфигурации приложение не стартует.
