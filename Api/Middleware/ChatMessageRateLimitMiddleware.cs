using System.Collections.Concurrent;

namespace Api.Middleware;

/// <summary>
/// In-memory rate-limit для POST /chats/{chatId}/messages.
/// </summary>
public sealed class ChatMessageRateLimitMiddleware
{
    private static readonly ConcurrentDictionary<string, Queue<DateTimeOffset>> Requests = new();
    private readonly RequestDelegate _next;
    private readonly ILogger<ChatMessageRateLimitMiddleware> _logger;
    private const int MaxRequestsPerMinute = 20;

    public ChatMessageRateLimitMiddleware(
        RequestDelegate next,
        ILogger<ChatMessageRateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsMessageSendRequest(context))
        {
            await _next(context);
            return;
        }

        var key = BuildKey(context);
        var queue = Requests.GetOrAdd(key, _ => new Queue<DateTimeOffset>());
        var now = DateTimeOffset.UtcNow;
        var isLimited = false;

        lock (queue)
        {
            while (queue.Count > 0 && (now - queue.Peek()).TotalMinutes >= 1)
            {
                queue.Dequeue();
            }

            if (queue.Count >= MaxRequestsPerMinute)
            {
                isLimited = true;
            }
            else
            {
                queue.Enqueue(now);
            }
        }

        if (isLimited)
        {
            _logger.LogWarning("Rate limit exceeded for key {RateLimitKey}", key);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new
            {
                error = new
                {
                    code = "rate_limited",
                    message = "Too many message requests. Try again later.",
                    traceId = context.TraceIdentifier
                }
            });
            return;
        }

        await _next(context);
    }

    private static bool IsMessageSendRequest(HttpContext context)
    {
        return HttpMethods.IsPost(context.Request.Method)
               && context.Request.Path.Value?.Contains("/chats/", StringComparison.OrdinalIgnoreCase) == true
               && context.Request.Path.Value?.EndsWith("/messages", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string BuildKey(HttpContext context)
    {
        var forwarded = context.Request.Headers["X-Forwarded-For"].ToString();
        var ip = string.IsNullOrWhiteSpace(forwarded)
            ? context.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip"
            : forwarded.Split(',')[0].Trim();
        var userId = context.Request.Query["userId"].ToString();
        return $"{ip}:{userId}";
    }
}
