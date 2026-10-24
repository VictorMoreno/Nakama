using Nakama.AnswerFeedback.Application;
using Nakama.AnswerFeedback.Application.Finder;
using Nakama.QuestionActivity.Application;
using Nakama.QuestionActivity.Application.Finder;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.UserActivity.Application;
using Nakama.UserActivity.Application.Finder;
using Nakama.Users.Application;
using Nakama.Users.Application.CountFinder;

namespace Nakama.Analytics.Application.Finder;

public static class AnalyticsFinder
{
    public sealed record Query : IQuery<AnalyticsResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, AnalyticsResponse>
    {
        public async Task<AnalyticsResponse> Handle(Query query, CancellationToken ct) => await finder.Execute(ct);
    }

    public sealed class Finder(IQueryBus queryBus) : IService
    {
        private const int ActiveUsersWindowDays = 7;

        private const int CurrentWeekOnly = 1;

        public async Task<AnalyticsResponse> Execute(CancellationToken cancellationToken)
        {
            UserActivityResponse activity = await queryBus.Ask(new UserActivityFinder.Query(ActiveUsersWindowDays), cancellationToken);
            QuestionActivityResponse questions = await queryBus.Ask(new QuestionActivityFinder.Query(CurrentWeekOnly), cancellationToken);
            QuestionsByChatbotResponse byChatbot = await queryBus.Ask(new QuestionsByChatbotFinder.Query(), cancellationToken);
            AnswerFeedbackSummaryResponse feedback = await queryBus.Ask(new AnswerFeedbackSummaryFinder.Query(), cancellationToken);
            UsersCountResponse users = await queryBus.Ask(new UsersCountFinder.Query(), cancellationToken);

            return AnalyticsResponse.From(activity.ActiveLast7Days, users.Total, questions.QuestionsThisWeek, byChatbot, feedback);
        }
    }
}
