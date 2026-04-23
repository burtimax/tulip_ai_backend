using Application.Services.Chat;
using FastEndpoints;

namespace Api.Endpoints.Chat;

public sealed class CreateChatEndpoint : Endpoint<CreateChatRequest, CreateChatResponse>
{
    private readonly IChatService _chatService;

    public CreateChatEndpoint(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override void Configure()
    {
        Post("/");
        Group<ChatGroupEndpoints>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateChatRequest req, CancellationToken ct)
    {
        if (req.UserId == Guid.Empty)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "userId is required" }, ct);
            return;
        }

        var chat = await _chatService.CreateChatAsync(req.UserId, req.Title, ct);
        await SendAsync(new CreateChatResponse
        {
            ChatId = chat.Id,
            Status = chat.Status.ToString(),
            CreatedAt = chat.CreatedAt
        }, cancellation: ct);
    }
}
