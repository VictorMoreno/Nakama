using Api.Configuration;
using Nakama.Shared.Domain;
using Nakama.Users.Application;
using Nakama.Users.Application.Searcher;

namespace Api.Endpoints;

public sealed class UsersEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("users", SearchUserSummaries)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithTags("Users")
            .WithSummary("Lists all users in the system (id, name, email) - e.g. to populate a team-assignment picker.")
            .Produces<IReadOnlyCollection<UserSummaryResponse>>();
    }

    private static async Task<IResult> SearchUserSummaries(IQueryBus queryBus, CancellationToken ct)
    {
        IReadOnlyCollection<UserSummaryResponse> response = await queryBus.Ask(new UserSummarySearcher.Query(), ct);
        return Results.Ok(response);
    }
}
