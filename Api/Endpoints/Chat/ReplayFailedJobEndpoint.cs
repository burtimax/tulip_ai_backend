using Application.Services.Chat;
using FastEndpoints;

namespace Api.Endpoints.Chat;

public sealed class ReplayFailedJobEndpoint : EndpointWithoutRequest
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
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var jobId = Route<Guid>("jobId");
        var replayed = await _chatService.ReplayFailedJobAsync(jobId, ct);
        if (!replayed)
        {
            await ChatEndpointErrors.WriteNotFoundAsync(HttpContext, "failed job not found", ct);
            return;
        }

        await HttpContext.Response.WriteAsJsonAsync(new { jobId, status = "Queued" }, ct);
    }
}
