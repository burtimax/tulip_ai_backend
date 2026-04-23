using Serilog.Context;

namespace Api.Middleware;

/// <summary>
/// Добавляет корреляционные идентификаторы в лог-контекст запроса.
/// </summary>
public sealed class RequestCorrelationMiddleware
{
    public const string CorrelationHeader = "X-Correlation-Id";
    public const string ChatHeader = "X-Chat-Id";
    public const string MessageHeader = "X-Message-Id";
    public const string JobHeader = "X-Job-Id";

    private readonly RequestDelegate _next;

    public RequestCorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveHeader(context, CorrelationHeader) ?? context.TraceIdentifier;
        context.Items[CorrelationHeader] = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;

        using var correlationScope = LogContext.PushProperty("CorrelationId", correlationId);
        using var chatScope = LogContext.PushProperty("ChatId", ResolveHeader(context, ChatHeader));
        using var messageScope = LogContext.PushProperty("MessageId", ResolveHeader(context, MessageHeader));
        using var jobScope = LogContext.PushProperty("JobId", ResolveHeader(context, JobHeader));

        await _next(context);
    }

    private static string? ResolveHeader(HttpContext context, string headerName)
    {
        if (!context.Request.Headers.TryGetValue(headerName, out var headerValues))
        {
            return null;
        }

        var value = headerValues.ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
