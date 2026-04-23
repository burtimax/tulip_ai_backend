using Application.Services.Chat;
using FastEndpoints;

namespace Api.Endpoints.Chat;

public sealed class GetMessagesEndpoint : EndpointWithoutRequest<MessagesResponse>
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
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var chatId = Route<Guid>("chatId");
        var skip = Math.Max(0, Query<int?>("skip") ?? 0);
        var take = Math.Clamp(Query<int?>("take") ?? 100, 1, 500);
        var messages = await _chatService.GetMessagesAsync(chatId, skip, take, ct);

        await SendAsync(new MessagesResponse
        {
            Items = messages.Select(x => new MessageDto
            {
                MessageId = x.Id,
                Role = x.Role.ToString(),
                Status = x.Status.ToString(),
                TextHtml = x.TextHtml,
                FailureCode = x.FailureCode,
                FailureReason = x.FailureReason,
                CreatedAt = x.CreatedAt,
                Images = x.Images.OrderBy(i => i.SortOrder).Select(i => new MessageImageDto
                {
                    Id = i.Id,
                    MimeType = i.MimeType,
                    SizeBytes = i.SizeBytes,
                    Width = i.Width,
                    Height = i.Height,
                    SortOrder = i.SortOrder
                }).ToList()
            }).ToList()
        }, cancellation: ct);
    }
}
