namespace ModuleS3.Service;

/// <summary>
/// Сервис для работы с S3-совместимым хранилищем (сохранение и получение файлов)
/// </summary>
public interface IS3ObjectStorageService
{
    /// <summary>
    /// Загрузка аудиофайла по filepath.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="filePath"></param>
    /// <param name="contentType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task UploadFileAsync(
        string key,
        string filePath,
        string? contentType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Загружает файл из потока в S3
    /// </summary>
    /// <param name="key">Ключ объекта в бакете (например, audio/guid.mp3)</param>
    /// <param name="stream">Поток с данными</param>
    /// <param name="contentType">MIME-тип (опционально)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task UploadFromStreamAsync(
        string key,
        Stream stream,
        string? contentType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Загружает файл из массива байтов в S3
    /// </summary>
    /// <param name="key">Ключ объекта в бакете</param>
    /// <param name="data">Данные файла</param>
    /// <param name="contentType">MIME-тип (опционально)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task UploadFromBytesAsync(
        string key,
        byte[] data,
        string? contentType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает содержимое объекта из S3 в виде массива байтов
    /// </summary>
    /// <param name="key">Ключ объекта в бакете</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Содержимое файла</returns>
    Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает поток для чтения объекта из S3
    /// </summary>
    /// <param name="key">Ключ объекта в бакете</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Поток для чтения</returns>
    Task<Stream> GetStreamAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удаляет объект из S3
    /// </summary>
    /// <param name="key">Ключ объекта в бакете</param>
    /// <param name="cancellationToken">Токен отмены</param>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет существование объекта в S3
    /// </summary>
    /// <param name="key">Ключ объекта в бакете</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>True, если объект существует</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Формирует полный ключ для аудиофайла по имени файла (с префиксом из конфигурации)
    /// </summary>
    /// <param name="storedFileName">Имя файла в хранилище (например, guid.mp3)</param>
    /// <returns>Ключ в S3 (например, audio/guid.mp3)</returns>
    string GetAudioFileKey(string storedFileName);
}
