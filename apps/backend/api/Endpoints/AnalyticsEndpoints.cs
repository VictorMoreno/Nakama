using Api.Configuration;
using Nakama.Analytics.Application;
using Nakama.Analytics.Application.FeedbackSubmitter;
using Nakama.Analytics.Application.Finder;
using Nakama.Shared.Domain;

namespace Api.Endpoints;

public sealed class AnalyticsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("analytics").WithTags("Analytics");

        group.MapGet("/", FindAnalytics)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Gets the global analytics snapshot.")
            .Produces<AnalyticsResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("feedback", SubmitFeedback)
            .WithSummary("Records whether a chat answer was useful, towards the global useful-answer rate.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> FindAnalytics(IQueryBus queryBus, CancellationToken ct) =>
        Results.Ok(await queryBus.Ask(new AnalyticsFinder.Query(), ct));

    private static async Task<IResult> SubmitFeedback(SubmitFeedbackRequest body, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(new AnalyticsFeedbackSubmitter.Command(body.Useful), ct);
        return Results.NoContent();
    }

    private sealed record SubmitFeedbackRequest(bool Useful);
}