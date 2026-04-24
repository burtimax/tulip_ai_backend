using Application.Services.Chat;
using FastEndpoints;
using Infrastructure.Db.App.Entities;
using Shared.Contracts;

namespace Api.Endpoints.Chat;

public sealed class GetMessageWithNextEndpoint : Endpoint<GetMessageWithNextRequest, Result<IReadOnlyList<MessageEntity>>>
{
    private readonly IChatService _chatService;

    public GetMessageWithNextEndpoint(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override void Configure()
    {
        Get("/messages/{messageId:guid}/next");
        Group<ChatGroupEndpoints>();
    }

    public override async Task HandleAsync(GetMessageWithNextRequest req, CancellationToken ct)
    {
        var messages = await _chatService.GetMessageWithNextAsync(req.MessageId, req.Take, ct);
        if (messages is null)
        {
            await ChatEndpointErrors.WriteNotFoundAsync(HttpContext, "message not found", ct);
            return;
        }

        await SendAsync(new(messages), cancellation: ct);
    }
}
