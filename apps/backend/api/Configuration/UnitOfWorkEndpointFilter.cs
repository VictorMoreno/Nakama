using Nakama.Shared.Domain;

namespace Api.Configuration;

public sealed class UnitOfWorkEndpointFilter(IUnitOfWork unitOfWork) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        object? result = await next(context);

        if (result is IStatusCodeHttpResult { StatusCode: >= StatusCodes.Status400BadRequest })
        {
            return result;
        }

        await unitOfWork.SaveChanges(CancellationToken.None);

        return result;
    }
}
