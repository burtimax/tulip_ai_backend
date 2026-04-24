using System.ComponentModel;
using Shared.Models;

namespace Api.Endpoints.Chat;

public sealed class CreateChatRequest
{
    [DefaultValue(null)]
    public string? Title { get; set; }
}

public sealed class GetChatsRequest : Pagination
{
}

public sealed class GetChatByIdRequest
{
    public Guid ChatId { get; set; }
}

public sealed class ReplayFailedJobRequest
{
    public Guid JobId { get; set; }
}

public sealed class ReplayFailedJobResponse
{
    public required Guid JobId { get; init; }
    public required string Status { get; init; }
}

public sealed class CreateChatResponse
{
    public required Guid ChatId { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class ChatListResponse
{
    public required IReadOnlyList<ChatItemDto> Items { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
}

public sealed class ChatItemDto
{
    public required Guid ChatId { get; init; }
    public required Guid UserId { get; init; }
    public string? Title { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public DateTimeOffset? LastMessageAt { get; init; }
    public bool IsProcessing { get; init; }
}

public sealed class SendMessageRequest
{
    public string? TextHtml { get; set; }
    public List<IFormFile> Images { get; set; } = new();
    public string? ClientRequestId { get; set; }
}

public sealed class SendMessageResponse
{
    public required Guid MessageId { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class GetMessagesRequest : Pagination
{
    public Guid ChatId { get; set; }
}

public sealed class GetMessageByIdRequest
{
    public Guid MessageId { get; set; }
}

