using Api.Configuration;
using Nakama.Shared.Domain;
using Nakama.Users.Application;
using Nakama.Users.Application.Creator;
using Nakama.Users.Application.Searcher;

namespace Api.Endpoints;

public sealed class UsersEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("users", CreateUser)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithTags("Users")
            .WithSummary("Admin-provisions a new user with an initial password.")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapGet("users", SearchUserSummaries)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithTags("Users")
            .WithSummary("Lists all users in the system (id, name, email) - e.g. to populate a team-assignment picker.")
            .Produces<IReadOnlyCollection<UserSummaryResponse>>();
    }

    private static async Task<IResult> CreateUser(CreateUserRequest body, ICommandBus commandBus, CancellationToken ct)
    {
        Guid id = Guid.NewGuid();
        await commandBus.Send(new UserCreator.Command(id, body.Email, body.Name, body.IsAdmin, body.InitialPassword), ct);
        return Results.Created();
    }

    private static async Task<IResult> SearchUserSummaries(IQueryBus queryBus, CancellationToken ct)
    {
        IReadOnlyCollection<UserSummaryResponse> response = await queryBus.Ask(new UserSummarySearcher.Query(), ct);
        return Results.Ok(response);
    }

    private sealed record CreateUserRequest(string Email, string Name, bool IsAdmin, string InitialPassword);
}
