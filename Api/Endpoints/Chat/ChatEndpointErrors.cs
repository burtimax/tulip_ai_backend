using Shared.Contracts;

namespace Api.Endpoints.Chat;

internal static class ChatEndpointErrors
{
    public static Task WriteValidationErrorAsync(HttpContext context, string message, string? code = null, CancellationToken cancellationToken = default)
    {
        return WriteErrorAsync(context, statusCode: 400, code ?? "validation_error", message, cancellationToken);
    }

    public static Task WriteNotFoundAsync(HttpContext context, string message, CancellationToken cancellationToken = default)
    {
        return WriteErrorAsync(context, statusCode: 404, "not_found", message, cancellationToken);
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string code, string message, CancellationToken cancellationToken)
    {
        var correlationId = context.Items.TryGetValue(Api.Middleware.RequestCorrelationMiddleware.CorrelationHeader, out var cid)
            ? cid?.ToString()
            : null;

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ApiErrorContract
        {
            Error = new ApiErrorBody
            {
                Code = code,
                Message = message,
                CorrelationId = correlationId,
                TraceId = context.TraceIdentifier
            }
        }, cancellationToken);
    }
}
