using Api.Configuration;
using Nakama.Analytics.Application;
using Nakama.Analytics.Application.FeedbackSubmitter;
using Nakama.Analytics.Application.Finder;
using Nakama.Shared.Domain;
using Nakama.UserActivity.Application;
using Nakama.UserActivity.Application.Finder;

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

        group.MapGet("user-activity", FindUserActivity)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Gets the daily user activity (logins and active people) of the last N days, plus the people active today, in 7 days and in 30 days.")
            .WithDescription(
                "An activity is a login or a session renewal, at most one renewal per person per configured interval. Days are bucketed in the configured time zone. `days` defaults to UserActivity:DefaultDays and cannot exceed UserActivity:MaxDays.")
            .Produces<UserActivityResponse>()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("feedback", SubmitFeedback)
            .WithSummary("Records whether a chat answer was useful, towards the global useful-answer rate.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> FindAnalytics(IQueryBus queryBus, CancellationToken ct) =>
        Results.Ok(await queryBus.Ask(new AnalyticsFinder.Query(), ct));

    private static async Task<IResult> FindUserActivity(IQueryBus queryBus, CancellationToken ct, int? days = null) =>
        Results.Ok(await queryBus.Ask(new UserActivityFinder.Query(days), ct));

    private static async Task<IResult> SubmitFeedback(SubmitFeedbackRequest body, ICommandBus commandBus, CancellationToken ct)
    {
        await commandBus.Send(new AnalyticsFeedbackSubmitter.Command(body.Useful), ct);
        return Results.NoContent();
    }

    private sealed record SubmitFeedbackRequest(bool Useful);
}