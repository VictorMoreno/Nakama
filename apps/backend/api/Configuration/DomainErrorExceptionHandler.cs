using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Nakama.Shared.Domain;

namespace Api.Configuration;

public sealed class DomainErrorExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
    {
        if (exception is not DomainError error)
        {
            return false;
        }

        var statusCode = error.Code switch
        {
            _ when error.Code.EndsWith(".not_found", StringComparison.Ordinal) => StatusCodes.Status404NotFound,
            _ when error.Code.EndsWith(".already_exists", StringComparison.Ordinal) => StatusCodes.Status409Conflict,
            _ when error.Code.EndsWith(".invalid_credentials", StringComparison.Ordinal) => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status422UnprocessableEntity,
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = error.Code,
                Detail = error.Message
            }, ct);

        return true;
    }
}
