using Application.Services.Chat;
using FastEndpoints;
using Infrastructure.Db.App.Entities;
using Infrastructure.Models;
using Shared.Contracts;

namespace Api.Endpoints.Chat;

public sealed class GetMessagesEndpoint : Endpoint<GetMessagesRequest, Result<PagedList<MessageEntity>>>
{
    private readonly IChatService _chatService;

    public GetMessagesEndpoint(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override void Configure()
    {
        Get("/{chatId:guid}/messages");
        Group<ChatGroupEndpoints>();
    }

    public override async Task HandleAsync(GetMessagesRequest req, CancellationToken ct)
    {
        var chatId = req.ChatId;
        var chat = await _chatService.GetChatAsync(chatId, ct);
        if (chat is null)
        {
            await ChatEndpointErrors.WriteNotFoundAsync(HttpContext, "chat not found", ct);
            return;
        }

        var pageNumber = Math.Max(1, req.PageNumber);
        var pageSize = Math.Clamp(req.PageSize, 1, 500);
        var messages = await _chatService.GetMessagesAsync(chatId, pageNumber, pageSize, ct);
        await SendAsync(new(messages), cancellation: ct);
    }
}
