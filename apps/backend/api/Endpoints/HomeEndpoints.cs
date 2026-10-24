using Api.Configuration;
using Nakama.Home.Application;
using Nakama.Home.Application.Finder;
using Nakama.Shared.Domain;

namespace Api.Endpoints;

public sealed class HomeEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("home").WithTags("Home");

        group.MapGet("/", FindHome)
            .WithSummary("Gets the caller's own usage summary: questions this week, active days in the last 30, and the progress of their active formations.")
            .WithDescription(
                "Only active, non-archived formations the caller is assigned to are listed. `pendingExams` holds the exams they have tried and not passed yet, with the score of their last attempt and the passing score. Days and weeks are bucketed in the configured time zone.")
            .Produces<HomeResponse>();
    }

    private static async Task<IResult> FindHome(ICurrentUser currentUser, IQueryBus queryBus, CancellationToken ct) =>
        Results.Ok(await queryBus.Ask(new HomeFinder.Query(currentUser.Id), ct));
}
