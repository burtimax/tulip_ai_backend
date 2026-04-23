using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Shared.Configs;

namespace ModuleS3.Service;

/// <summary>
/// Реализация сервиса для работы с S3-совместимым хранилищем (Timeweb Cloud и др.)
/// </summary>
public class S3ObjectStorageService : IS3ObjectStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Configuration _config;
    private readonly ILogger<S3ObjectStorageService> _logger;

    public S3ObjectStorageService(
        S3Configuration options,
        ILogger<S3ObjectStorageService> logger)
    {
        _config = options ?? throw new InvalidOperationException("Конфигурация S3 не задана (секция S3 в appsettings).");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _s3Client = new AmazonS3Client(
            _config.AccessKey,
            _config.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = _config.ServiceUrl,
                ForcePathStyle = _config.ForcePathStyle,
                AuthenticationRegion = string.IsNullOrEmpty(_config.Region) ? "ru-1" : _config.Region
            });
    }

    public async Task UploadFileAsync(
        string key,
        string filePath,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open);
        await UploadFromStreamAsync(key, fileStream, contentType, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UploadFromStreamAsync(
        string key,
        Stream stream,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым", nameof(key));
        ArgumentNullException.ThrowIfNull(stream);

        var request = new PutObjectRequest
        {
            BucketName = _config.BucketName,
            Key = key,
            InputStream = stream,
            AutoCloseStream = false
        };
        if (!string.IsNullOrEmpty(contentType))
            request.ContentType = contentType;

        var res = await _s3Client.PutObjectAsync(request, cancellationToken);
        _logger.LogDebug("Файл загружен в S3: {Key}", key);
    }

    /// <inheritdoc />
    public async Task UploadFromBytesAsync(
        string key,
        byte[] data,
        string? contentType = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым", nameof(key));
        ArgumentNullException.ThrowIfNull(data);

        await using var stream = new MemoryStream(data);
        await UploadFromStreamAsync(key, stream, contentType, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<byte[]> GetBytesAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым", nameof(key));

        var request = new GetObjectRequest
        {
            BucketName = _config.BucketName,
            Key = key
        };

        using var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        await using var ms = new MemoryStream();
        await response.ResponseStream.CopyToAsync(ms, cancellationToken);
        return ms.ToArray();
    }

    /// <inheritdoc />
    public async Task<Stream> GetStreamAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым", nameof(key));

        var request = new GetObjectRequest
        {
            BucketName = _config.BucketName,
            Key = key
        };

        var response = await _s3Client.GetObjectAsync(request, cancellationToken);
        return response.ResponseStream;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым", nameof(key));

        var request = new DeleteObjectRequest
        {
            BucketName = _config.BucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);
        _logger.LogDebug("Объект удалён из S3: {Key}", key);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Ключ не может быть пустым", nameof(key));

        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _config.BucketName,
                Key = key
            };
            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public string GetAudioFileKey(string storedFileName)
    {
        if (string.IsNullOrWhiteSpace(storedFileName))
            throw new ArgumentException("Имя файла не может быть пустым", nameof(storedFileName));

        var prefix = (_config.AudioFilesKeyPrefix ?? "").Trim().TrimEnd('/');
        return string.IsNullOrEmpty(prefix)
            ? storedFileName
            : $"{prefix}/{storedFileName}";
    }
}
