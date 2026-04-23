using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModuleS3.Service;
using Shared.Configs;

namespace ModuleS3.Extensions;

/// <summary>
/// Расширения для регистрации S3-сервиса в DI
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет конфигурацию S3 и сервис S3-совместимого хранилища в контейнер.
    /// Секция конфигурации по умолчанию — "S3".
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configuration">Конфигурация приложения (для секции S3)</param>
    /// <returns>Коллекция сервисов для цепочки вызовов</returns>
    public static IServiceCollection AddS3ObjectStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var config = configuration.GetSection("S3").Get<S3Configuration>();
        if (config is null) throw new Exception("S3 configuration is missing");

        services.AddSingleton(config);
        services.AddScoped<IS3ObjectStorageService, S3ObjectStorageService>();
        return services;
    }
}
