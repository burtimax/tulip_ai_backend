using Application.Services.Chat;
using FastEndpoints;
using Infrastructure.Db.App.Entities;
using Shared.Contracts;

namespace Api.Endpoints.Chat;

public sealed class GetMessageByIdEndpoint : Endpoint<GetMessageByIdRequest, Result<MessageEntity>>
{
    private readonly IChatService _chatService;

    public GetMessageByIdEndpoint(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override void Configure()
    {
        Get("/messages/{messageId:guid}");
        Group<ChatGroupEndpoints>();
    }

    public override async Task HandleAsync(GetMessageByIdRequest req, CancellationToken ct)
    {
        var message = await _chatService.GetMessageAsync(req.MessageId, ct);
        if (message is null)
        {
            await ChatEndpointErrors.WriteNotFoundAsync(HttpContext, "message not found", ct);
            return;
        }

        await SendAsync(new(message), cancellation: ct);
    }
}
