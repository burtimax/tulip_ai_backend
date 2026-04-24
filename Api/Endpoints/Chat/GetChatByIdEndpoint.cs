using Application.Services.Chat;
using FastEndpoints;
using Shared.Contracts;

namespace Api.Endpoints.Chat;

public sealed class GetChatByIdEndpoint : Endpoint<GetChatByIdRequest, Result<ChatItemDto>>
{
    private readonly IChatService _chatService;

    public GetChatByIdEndpoint(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override void Configure()
    {
        Get("/{chatId:guid}");
        Group<ChatGroupEndpoints>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Получение чата по ИД";
            s.Description = "Получаем конкретный чат по идентификатору";
        });
    }

    public override async Task HandleAsync(GetChatByIdRequest req, CancellationToken ct)
    {
        var chat = await _chatService.GetChatAsync(req.ChatId, ct);
        if (chat is null)
        {
            await ChatEndpointErrors.WriteNotFoundAsync(HttpContext, "chat not found", ct);
            return;
        }

        await SendAsync(new Result<ChatItemDto>(new ChatItemDto
        {
            ChatId = chat.Id,
            UserId = chat.UserId,
            Title = chat.Title,
            Status = chat.Status.ToString(),
            CreatedAt = chat.CreatedAt,
            UpdatedAt = chat.UpdatedAt,
            LastMessageAt = chat.LastMessageAt,
            IsProcessing = string.Equals(chat.Status.ToString(), "Processing", StringComparison.Ordinal)
        }), cancellation: ct);
    }
}
