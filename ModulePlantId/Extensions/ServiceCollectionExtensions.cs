using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulePlantId.Configuration;
using ModulePlantId.Services;
using Shared.Configs;

namespace ModulePlantId.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModulePlantId(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var plantIdConfig = configuration
            .GetSection(PlantIdApiConfiguration.Section)
            .Get<PlantIdApiConfiguration>();

        if (plantIdConfig is null)
            throw new Exception("PlantId configuration is missing");

        var proxyConfig = configuration.GetSection(ProxyConfiguration.Section).Get<ProxyConfiguration>()
            ?? new ProxyConfiguration();

        services.AddHttpClient();
        services.AddSingleton(proxyConfig);
        services.AddSingleton(plantIdConfig);
        if (plantIdConfig.UseMockService)
        {
            services.AddTransient<IPlantIdService, MockPlantIdService>();
        }
        else
        {
            services.AddTransient<IPlantIdService, PlantIdService>();
        }

        return services;
    }
}
