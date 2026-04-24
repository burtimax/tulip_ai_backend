using Api.Extensions;
using Application.Services.Chat;
using FastEndpoints;
using Shared.Contracts;

namespace Api.Endpoints.Chat;

public sealed class CreateChatEndpoint : Endpoint<CreateChatRequest, Result<CreateChatResponse>>
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
    }

    public override async Task HandleAsync(CreateChatRequest req, CancellationToken ct)
    {
        var userId = HttpContext.TokenData().UserId;
        var title = DateTimeOffset.UtcNow.ToString();

        var chat = await _chatService.CreateChatAsync(userId, title, ct);
        await SendAsync(new Result<CreateChatResponse>(new CreateChatResponse
        {
            ChatId = chat.Id,
            Status = chat.Status.ToString(),
            CreatedAt = chat.CreatedAt
        }), cancellation: ct);
    }
}
