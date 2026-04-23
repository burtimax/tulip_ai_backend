using Application.Services.Chat;
using FastEndpoints;

namespace Api.Endpoints.Chat;

public sealed class GetChatsEndpoint : EndpointWithoutRequest<ChatListResponse>
{
    private readonly IChatService _chatService;

    public GetChatsEndpoint(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override void Configure()
    {
        Get("/");
        Group<ChatGroupEndpoints>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(Query<string>("userId"), out var userId) || userId == Guid.Empty)
        {
            HttpContext.Response.StatusCode = 400;
            await HttpContext.Response.WriteAsJsonAsync(new { error = "userId is required" }, ct);
            return;
        }

        var skip = Math.Max(0, Query<int?>("skip") ?? 0);
        var take = Math.Clamp(Query<int?>("take") ?? 50, 1, 200);
        var chats = await _chatService.GetChatsAsync(userId, skip, take, ct);

        await SendAsync(new ChatListResponse
        {
            Items = chats.Select(x => new ChatItemDto
            {
                ChatId = x.Id,
                UserId = x.UserId,
                Title = x.Title,
                Status = x.Status.ToString(),
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                LastMessageAt = x.LastMessageAt
            }).ToList()
        }, cancellation: ct);
    }
}
