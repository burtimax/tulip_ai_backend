namespace Shared.Contracts;

/// <summary>
/// Единый формат ошибок API/worker.
/// </summary>
public sealed class ApiErrorContract
{
    public required ApiErrorBody Error { get; init; }
}

public sealed class ApiErrorBody
{
    public required string Code { get; init; }
    public required string Message { get; init; }
    public string? CorrelationId { get; init; }
    public string? TraceId { get; init; }
}
