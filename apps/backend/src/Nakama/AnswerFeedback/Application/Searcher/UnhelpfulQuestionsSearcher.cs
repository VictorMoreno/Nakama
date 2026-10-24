using Microsoft.Extensions.Options;
using Nakama.AnswerFeedback.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.AnswerFeedback;

namespace Nakama.AnswerFeedback.Application.Searcher;

public static class UnhelpfulQuestionsSearcher
{
    public sealed record Query(Guid BotKey) : IQuery<UnhelpfulQuestionsResponse>;

    public sealed class Handler(Searcher searcher) : IQueryHandler<Query, UnhelpfulQuestionsResponse>
    {
        public async Task<UnhelpfulQuestionsResponse> Handle(Query query, CancellationToken ct) =>
            await searcher.Execute(query.BotKey, ct);
    }

    public sealed class Searcher(
        IAnswerFeedbackRepository repository,
        IReportingCalendar calendar,
        IOptions<AnswerFeedbackOptions> options) : IService
    {
        public async Task<UnhelpfulQuestionsResponse> Execute(Guid botKey, CancellationToken cancellationToken)
        {
            DateOnly today = calendar.Today();
            DateOnly weekStart = calendar.WeekStart(today);

            IReadOnlyList<AnswerFeedbackEntry> entries = await repository.Search(
                new UnhelpfulAnswersOfChatbotBetweenSpecification(
                    botKey,
                    calendar.StartOfDayUtc(weekStart),
                    calendar.StartOfDayUtc(today.AddDays(1))),
                cancellationToken);

            return UnhelpfulQuestionsGrouper.Group(entries, options.Value.UnhelpfulQuestionsLimit);
        }
    }
}
