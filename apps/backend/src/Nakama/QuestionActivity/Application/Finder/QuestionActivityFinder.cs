using Microsoft.Extensions.Options;
using Nakama.QuestionActivity.Domain;
using Nakama.QuestionActivity.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.QuestionActivity;

namespace Nakama.QuestionActivity.Application.Finder;

public static class QuestionActivityFinder
{
    public sealed record Query(int? Weeks) : IQuery<QuestionActivityResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, QuestionActivityResponse>
    {
        public async Task<QuestionActivityResponse> Handle(Query query, CancellationToken ct) =>
            await finder.Execute(query.Weeks, ct);
    }

    public sealed class Finder(
        IQuestionActivityRepository repository,
        ISystemClock systemClock,
        IOptions<QuestionActivityOptions> options) : IService
    {
        private const int DaysPerWeek = 7;

        public async Task<QuestionActivityResponse> Execute(int? requestedWeeks, CancellationToken cancellationToken)
        {
            QuestionActivityOptions settings = options.Value;
            int weeks = requestedWeeks ?? settings.DefaultWeeks;

            if (weeks < 1 || weeks > settings.MaxWeeks)
            {
                throw QuestionActivityErrors.InvalidWeeks(settings.MaxWeeks);
            }

            TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(settings.TimeZone);
            DateOnly today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(systemClock.UtcNow, zone));
            DateOnly currentWeek = WeekStart(today);
            DateOnly firstWeek = currentWeek.AddDays(-DaysPerWeek * (weeks - 1));

            IReadOnlyList<QuestionActivityEntry> entries = await repository.Search(
                new QuestionActivityBetweenSpecification(StartOfDayUtc(firstWeek, zone), StartOfDayUtc(today.AddDays(1), zone)),
                cancellationToken);
            Dictionary<DateOnly, int> perWeek = entries
                .GroupBy(entry => WeekStart(LocalDay(entry.OccurredAt, zone)))
                .ToDictionary(group => group.Key, group => group.Count());

            List<WeeklyQuestionRow> rows = Enumerable.Range(0, weeks)
                .Select(offset => firstWeek.AddDays(DaysPerWeek * offset))
                .Select(week => new WeeklyQuestionRow(week.ToString("yyyy-MM-dd"), perWeek.GetValueOrDefault(week)))
                .ToList();

            return new QuestionActivityResponse(rows, perWeek.GetValueOrDefault(currentWeek));
        }

        private static DateOnly WeekStart(DateOnly day) => day.AddDays(-(((int)day.DayOfWeek + 6) % DaysPerWeek));

        private static DateOnly LocalDay(DateTime occurredAtUtc, TimeZoneInfo zone) =>
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(occurredAtUtc, DateTimeKind.Utc), zone));

        private static DateTime StartOfDayUtc(DateOnly day, TimeZoneInfo zone) =>
            TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), zone);
    }
}
