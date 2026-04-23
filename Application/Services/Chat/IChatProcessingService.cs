namespace Application.Services.Chat;

public interface IChatProcessingService
{
    Task<int> ProcessQueuedJobsAsync(CancellationToken cancellationToken = default);
}
