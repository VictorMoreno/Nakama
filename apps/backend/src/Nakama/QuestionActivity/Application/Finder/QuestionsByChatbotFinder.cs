using Microsoft.Extensions.Options;
using Nakama.QuestionActivity.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.QuestionActivity;

namespace Nakama.QuestionActivity.Application.Finder;

public static class QuestionsByChatbotFinder
{
    public sealed record Query : IQuery<QuestionsByChatbotResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, QuestionsByChatbotResponse>
    {
        public async Task<QuestionsByChatbotResponse> Handle(Query query, CancellationToken ct) =>
            await finder.Execute(ct);
    }

    public sealed class Finder(
        IQuestionActivityRepository repository,
        ISystemClock systemClock,
        IOptions<QuestionActivityOptions> options) : IService
    {
        public async Task<QuestionsByChatbotResponse> Execute(CancellationToken cancellationToken)
        {
            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);
            DateOnly today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(systemClock.UtcNow, zone));
            DateOnly firstDayOfMonth = new(today.Year, today.Month, 1);

            IReadOnlyList<QuestionActivityEntry> entries = await repository.Search(
                new QuestionActivityBetweenSpecification(StartOfDayUtc(firstDayOfMonth, zone), StartOfDayUtc(today.AddDays(1), zone)),
                cancellationToken);

            List<ChatbotQuestionsRow> rows = entries
                .GroupBy(entry => entry.BotKey)
                .Select(group => new ChatbotQuestionsRow(group.Key, group.Count()))
                .OrderByDescending(row => row.Questions)
                .ThenBy(row => row.BotKey)
                .ToList();

            return new QuestionsByChatbotResponse(rows);
        }

        private static DateTime StartOfDayUtc(DateOnly day, TimeZoneInfo zone) =>
            TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), zone);
    }
}
