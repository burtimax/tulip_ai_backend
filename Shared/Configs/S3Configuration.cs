namespace Shared.Configs;

/// <summary>
/// Конфигурация подключения к S3-совместимому хранилищу (Timeweb Cloud и др.)
/// </summary>
public class S3Configuration
{
    /// <summary>
    /// URL сервиса S3 (например, https://s3.timeweb.cloud для Timeweb)
    /// </summary>
    public string ServiceUrl { get; set; } = "https://s3.timeweb.cloud";

    /// <summary>
    /// Имя бакета
    /// </summary>
    public string BucketName { get; set; } = "";

    /// <summary>
    /// Access Key (идентификатор доступа)
    /// </summary>
    public string AccessKey { get; set; } = "";

    /// <summary>
    /// Secret Key (секретный ключ)
    /// </summary>
    public string SecretKey { get; set; } = "";

    /// <summary>
    /// Префикс для файлов сервиса
    /// </summary>
    public string Prefix { get; set; } = "";

    /// <summary>
    /// Регион (опционально, для Timeweb может быть ru-1 и т.д.)
    /// </summary>
    public string Region { get; set; } = "ru-1";

    /// <summary>
    /// Использовать path-style для URL (рекомендуется для кастомных S3-эндпоинтов, например Timeweb)
    /// </summary>
    public bool ForcePathStyle { get; set; } = true;

    /// <summary>
    /// Префикс ключа для аудиофайлов в бакете (например, "images")
    /// </summary>
    public string AudioFilesKeyPrefix { get; set; } = "images";
}
