using Api.Configuration;
using Nakama.Analytics.Application;
using Nakama.Analytics.Application.Finder;
using Nakama.Shared.Domain;

namespace Api.Endpoints;

public sealed class AnalyticsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("analytics").WithTags("Analytics");

        group.MapGet("/", FindAnalytics)
            .WithSummary("Gets the global analytics snapshot.")
            .Produces<AnalyticsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> FindAnalytics(IQueryBus queryBus, CancellationToken ct) =>
        Results.Ok(await queryBus.Ask(new AnalyticsFinder.Query(), ct));
}