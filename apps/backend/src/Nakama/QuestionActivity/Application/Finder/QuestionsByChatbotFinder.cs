using Nakama.QuestionActivity.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.QuestionActivity.Application.Finder;

public static class QuestionsByChatbotFinder
{
    public sealed record Query : IQuery<QuestionsByChatbotResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, QuestionsByChatbotResponse>
    {
        public async Task<QuestionsByChatbotResponse> Handle(Query query, CancellationToken ct) =>
            await finder.Execute(ct);
    }

    public sealed class Finder(IQuestionActivityRepository repository, IReportingCalendar calendar) : IService
    {
        public async Task<QuestionsByChatbotResponse> Execute(CancellationToken cancellationToken)
        {
            DateOnly today = calendar.Today();
            DateOnly firstDayOfMonth = new(today.Year, today.Month, 1);

            IReadOnlyList<QuestionActivityEntry> entries = await repository.Search(
                new QuestionActivityBetweenSpecification(calendar.StartOfDayUtc(firstDayOfMonth), calendar.StartOfDayUtc(today.AddDays(1))),
                cancellationToken);

            List<ChatbotQuestionsRow> rows = entries
                .GroupBy(entry => entry.BotKey)
                .Select(group => new ChatbotQuestionsRow(group.Key, group.Count()))
                .OrderByDescending(row => row.Questions)
                .ThenBy(row => row.BotKey)
                .ToList();

            return new QuestionsByChatbotResponse(rows);
        }
    }
}
