using Application.Services.Chat;
using FastEndpoints;
using Shared.Contracts;

namespace Api.Endpoints.Chat;

public sealed class ReplayFailedJobEndpoint : Endpoint<ReplayFailedJobRequest, Result<ReplayFailedJobResponse>>
{
    private readonly IChatService _chatService;

    public ReplayFailedJobEndpoint(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override void Configure()
    {
        Post("/jobs/{jobId:guid}/replay");
        Group<ChatGroupEndpoints>();
    }

    public override async Task HandleAsync(ReplayFailedJobRequest req, CancellationToken ct)
    {
        var replayed = await _chatService.ReplayFailedJobAsync(req.JobId, ct);
        if (!replayed)
        {
            await ChatEndpointErrors.WriteNotFoundAsync(HttpContext, "failed job not found", ct);
            return;
        }

        await SendAsync(new(new ReplayFailedJobResponse
        {
            JobId = req.JobId,
            Status = "Queued"
        }), cancellation: ct);
    }
}
