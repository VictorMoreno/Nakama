using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nakama.Shared.Domain;

namespace Api.Configuration;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not DomainError error)
        {
            logger.LogError(
                exception,
                "Unhandled exception on {Method} {Path} (trace {TraceId})",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier);

            return false;
        }

        int statusCode = error.Code switch
        {
            _ when error.Code.EndsWith(".not_found", StringComparison.Ordinal) => StatusCodes.Status404NotFound,
            _ when error.Code.EndsWith(".already_exists", StringComparison.Ordinal) => StatusCodes.Status409Conflict,
            _ when error.Code.EndsWith(".invalid_credentials", StringComparison.Ordinal) => StatusCodes.Status401Unauthorized,
            _ when error.Code.EndsWith(".invalid_refresh_token", StringComparison.Ordinal) => StatusCodes.Status401Unauthorized,
            _ when error.Code.EndsWith(".refresh_token_reused", StringComparison.Ordinal) => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status422UnprocessableEntity
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = error.Code,
                Detail = error.Message
            }, cancellationToken);

        return true;
    }
}
