using Application.Services.Chat;
using Microsoft.Extensions.Options;
using Shared.Configs;

namespace Api.BackgroundServices;

public sealed class ChatProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ChatQueueConfiguration _queueOptions;
    private readonly ILogger<ChatProcessingWorker> _logger;

    public ChatProcessingWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<ChatQueueConfiguration> queueOptions,
        ILogger<ChatProcessingWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _queueOptions = queueOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Chat processing worker started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IChatProcessingService>();
                await service.ProcessQueuedJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in chat processing worker loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(_queueOptions.PollIntervalSeconds), stoppingToken);
        }
    }
}
