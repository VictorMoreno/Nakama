using Api.Configuration;
using Nakama.Analytics.Application;
using Nakama.Analytics.Application.Finder;
using Nakama.AnswerFeedback.Application;
using Nakama.AnswerFeedback.Application.Finder;
using Nakama.QuestionActivity.Application;
using Nakama.QuestionActivity.Application.Finder;
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
                "An activity is a login or a session renewal, at most one renewal per person per configured interval. Days are bucketed in the configured time zone. `days` defaults to Reporting:UserActivity:DefaultDays and cannot exceed Reporting:UserActivity:MaxDays.")
            .Produces<UserActivityResponse>()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("question-activity", FindQuestionActivity)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Gets the number of questions asked to the chatbots per week, for the last N weeks, plus the ones asked this week.")
            .WithDescription(
                "Weeks start on Monday and are bucketed in the configured time zone; the last row is the current week. `weeks` defaults to Reporting:QuestionActivity:DefaultWeeks and cannot exceed Reporting:QuestionActivity:MaxWeeks.")
            .Produces<QuestionActivityResponse>()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("unhelpful-questions", FindUnhelpfulQuestions)
            .RequireAuthorization(AuthPolicies.Admin)
            .WithSummary("Gets the questions whose answer was rated as not useful this week, grouped by chatbot and question, most repeated first.")
            .WithDescription(
                "The week starts on Monday in the configured time zone. Questions are grouped ignoring case and surrounding spaces, no person is identified, and at most Reporting:AnswerFeedback:UnhelpfulQuestionsLimit rows are returned.")
            .Produces<UnhelpfulQuestionsResponse>();
    }

    private static async Task<IResult> FindAnalytics(IQueryBus queryBus, CancellationToken ct) =>
        Results.Ok(await queryBus.Ask(new AnalyticsFinder.Query(), ct));

    private static async Task<IResult> FindUserActivity(IQueryBus queryBus, CancellationToken ct, int? days = null) =>
        Results.Ok(await queryBus.Ask(new UserActivityFinder.Query(days), ct));

    private static async Task<IResult> FindQuestionActivity(IQueryBus queryBus, CancellationToken ct, int? weeks = null) =>
        Results.Ok(await queryBus.Ask(new QuestionActivityFinder.Query(weeks), ct));

    private static async Task<IResult> FindUnhelpfulQuestions(IQueryBus queryBus, CancellationToken ct) =>
        Results.Ok(await queryBus.Ask(new UnhelpfulQuestionsFinder.Query(), ct));
}