using System;
using Application.Extensions;

using Application.Services.BootstrapDatabase;
using Application.Services.Chat;
using Application.Services.Email;
using Application.Services.Llm;
using Application.Services.Knowledge;
using Application.Services.StatEvent;
using Application.Services.User;
using Application.Services.FrontendLog;
using Application.QuartzJobs;
using Application.Utils;
using Infrastructure.Db.App;
using Infrastructure.Db.App.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModuleLLM.Configuration;
using ModulePlantId.Configuration;
using Quartz;
using Shared.Configs;
using Shared.Contracts;

namespace Api.Extensions;

public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет планировщик задач Quartz для рассылок.
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    public static void AddQuartzHostedService(this IServiceCollection services)
    {
        services.AddQuartz(quartzConfigurator =>
        {
            quartzConfigurator.UseMicrosoftDependencyInjectionJobFactory();
        });
        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });
    }



    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions(configuration);

        services.AddJwt(configuration);
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IStatEventService, StatEventService>();
        services.AddScoped<IFrontendLogService, FrontendLogService>();
        services.AddScoped<IChatService, ChatService>();
        services.AddScoped<IChatProcessingService, ChatProcessingService>();
        services.AddScoped<IKnowledgeService, KnowledgeService>();
        services.AddSingleton<ChatProcessingMetrics>();
        services.AddTransient<TestJob>();
        services.AddScoped<IDatabaseBootstrap, DatabaseBootstrap>();

        //services.AddScoped<ILlmService, LlmService>();
        //services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddScoped<SetByUserIdInterceptor>();
        services.AddSingleton<ILlmUsageJournal, LlmUsageJournalService>();
        services.AddHostedService<Api.BackgroundServices.ChatProcessingWorker>();
    }

    private static void AddValidatedOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ProcessorJobOptions>()
            .Bind(configuration.GetSection(ProcessorJobOptions.SectionName))
            .Validate(
                x => x.MaxParallelGenerations > 0,
                $"{ProcessorJobOptions.SectionName}:MaxParallelGenerations must be greater than 0")
            .ValidateOnStart();

        services.AddOptions<ChatQueueConfiguration>()
            .Bind(configuration.GetSection(ChatQueueConfiguration.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ChatStorageConfiguration>()
            .Bind(configuration.GetSection(ChatStorageConfiguration.Section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<OpenRouterApiConfiguration>()
            .Bind(configuration.GetSection(OpenRouterApiConfiguration.Section))
            .Validate(
                x => !string.IsNullOrWhiteSpace(x.BaseUrl) && Uri.IsWellFormedUriString(x.BaseUrl, UriKind.Absolute),
                $"{OpenRouterApiConfiguration.Section}:BaseUrl must be an absolute URI")
            .Validate(
                x => x.Timeout > 0,
                $"{OpenRouterApiConfiguration.Section}:Timeout must be greater than 0")
            .ValidateOnStart();

        services.AddOptions<PlantIdApiConfiguration>()
            .Bind(configuration.GetSection(PlantIdApiConfiguration.Section))
            .Validate(
                x => !string.IsNullOrWhiteSpace(x.BaseUrl) && Uri.IsWellFormedUriString(x.BaseUrl, UriKind.Absolute),
                $"{PlantIdApiConfiguration.Section}:BaseUrl must be an absolute URI")
            .Validate(
                x => !string.IsNullOrWhiteSpace(x.AnalyzeEndpoint),
                $"{PlantIdApiConfiguration.Section}:AnalyzeEndpoint is required")
            .ValidateOnStart();

        services.AddOptions<ProxyConfiguration>()
            .Bind(configuration.GetSection(ProxyConfiguration.Section))
            .Validate(
                x => !x.Enabled || (!string.IsNullOrWhiteSpace(x.Host) && x.Port > 0),
                $"{ProxyConfiguration.Section}:Host and Port are required when Enabled=true")
            .ValidateOnStart();
    }

    public static AppConfiguration AddConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        AppConfiguration config = configuration.Get<AppConfiguration>();
        if(config == null) throw new NullReferenceException(nameof(config));
        if (string.IsNullOrWhiteSpace(config.Database?.AppDbConnection))
            throw new OptionsValidationException(
                nameof(AppConfiguration),
                typeof(AppConfiguration),
                new[] { "Database:AppDbConnection is required and must be set via environment variables or secrets." });

        services.AddSingleton(config);

        return config;
    }

    /// <summary>
    /// Регистрирует контекст базы данных в DI контейнере
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="config">Конфигурация подключения к БД</param>
    /// <param name="environment">Информация об окружении приложения</param>
    public static void AddDatabase(
        this IServiceCollection services,
        DatabaseAppConfiguration config,
        IHostEnvironment environment)
    {
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                options.UseNpgsql(config.AppDbConnection);
                options.AddInterceptors(serviceProvider.GetRequiredService<SetByUserIdInterceptor>());
                // ВАЖНО: EnableSensitiveDataLogging включается только в режиме разработки
                // В production это может привести к утечке конфиденциальных данных в логах
                if (environment.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            }
        );
    }
}
