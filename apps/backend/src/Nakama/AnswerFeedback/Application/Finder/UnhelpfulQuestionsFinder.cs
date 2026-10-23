using Microsoft.Extensions.Options;
using Nakama.AnswerFeedback.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.AnswerFeedback;

namespace Nakama.AnswerFeedback.Application.Finder;

public static class UnhelpfulQuestionsFinder
{
    public sealed record Query : IQuery<UnhelpfulQuestionsResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, UnhelpfulQuestionsResponse>
    {
        public async Task<UnhelpfulQuestionsResponse> Handle(Query query, CancellationToken ct) =>
            await finder.Execute(ct);
    }

    public sealed class Finder(
        IAnswerFeedbackRepository repository,
        IReportingCalendar calendar,
        IOptions<AnswerFeedbackOptions> options) : IService
    {
        public async Task<UnhelpfulQuestionsResponse> Execute(CancellationToken cancellationToken)
        {
            DateOnly today = calendar.Today();
            DateOnly weekStart = calendar.WeekStart(today);

            IReadOnlyList<AnswerFeedbackEntry> entries = await repository.Search(
                new UnhelpfulAnswersBetweenSpecification(calendar.StartOfDayUtc(weekStart), calendar.StartOfDayUtc(today.AddDays(1))),
                cancellationToken);

            List<UnhelpfulQuestionRow> rows = entries
                .GroupBy(entry => (entry.BotKey, Key: entry.Question.Trim().ToLowerInvariant()))
                .Select(group => new UnhelpfulQuestionRow(group.Key.BotKey, group.First().Question.Trim(), group.Count()))
                .OrderByDescending(row => row.Count)
                .ThenBy(row => row.Question, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.BotKey)
                .Take(options.Value.UnhelpfulQuestionsLimit)
                .ToList();

            return new UnhelpfulQuestionsResponse(rows);
        }
    }
}
