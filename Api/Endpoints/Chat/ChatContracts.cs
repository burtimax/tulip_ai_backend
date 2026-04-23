namespace Api.Endpoints.Chat;

public sealed class CreateChatRequest
{
    public Guid UserId { get; set; }
    public string? Title { get; set; }
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
    public int Skip { get; init; }
    public int Take { get; init; }
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
    public List<IncomingImageDto> Images { get; set; } = new();
    public string? ClientRequestId { get; set; }
}

public sealed class IncomingImageDto
{
    public string? DataUrl { get; set; }
    public string? MimeType { get; set; }
    public long SizeBytes { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}

public sealed class SendMessageResponse
{
    public required Guid MessageId { get; init; }
    public required string Status { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
}

public sealed class MessagesResponse
{
    public required Guid ChatId { get; init; }
    public required string ChatStatus { get; init; }
    public bool IsChatProcessing { get; init; }
    public required IReadOnlyList<MessageDto> Items { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
}

public sealed class MessageDto
{
    public required Guid MessageId { get; init; }
    public required string Role { get; init; }
    public required string Status { get; init; }
    public string? TextHtml { get; init; }
    public string? FailureCode { get; init; }
    public string? FailureReason { get; init; }
    public bool HasProcessingError { get; init; }
    public bool CanRetry { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required IReadOnlyList<MessageImageDto> Images { get; init; }
}

public sealed class MessageImageDto
{
    public required Guid Id { get; init; }
    public required string MimeType { get; init; }
    public required long SizeBytes { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public required int SortOrder { get; init; }
}
