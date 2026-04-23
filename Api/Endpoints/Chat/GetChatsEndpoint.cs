using Api.Extensions;
using Application.Services.Chat;
using FastEndpoints;
using Infrastructure.Db.App.Entities;
using Infrastructure.Models;
using Shared.Contracts;

namespace Api.Endpoints.Chat;

public sealed class GetChatsEndpoint : Endpoint<GetChatsRequest, Result<PagedList<ChatEntity>>>
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
    }

    public override async Task HandleAsync(GetChatsRequest req, CancellationToken ct)
    {
        var userId = HttpContext.TokenData().UserId;
        var pageNumber = Math.Max(1, req.PageNumber);
        var pageSize = Math.Clamp(req.PageSize, 1, 200);
        var chats = await _chatService.GetChatsAsync(userId, pageNumber, pageSize, ct);
        await SendAsync(new(chats), cancellation: ct);
    }
}
