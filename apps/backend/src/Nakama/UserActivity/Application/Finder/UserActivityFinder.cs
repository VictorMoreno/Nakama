using Microsoft.Extensions.Options;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.UserActivity;
using Nakama.UserActivity.Domain;
using Nakama.UserActivity.Domain.Errors;

namespace Nakama.UserActivity.Application.Finder;

public static class UserActivityFinder
{
    public sealed record Query(int? Days, Guid? UserId = null) : IQuery<UserActivityResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, UserActivityResponse>
    {
        public async Task<UserActivityResponse> Handle(Query query, CancellationToken ct) =>
            await finder.Execute(query.Days, query.UserId, ct);
    }

    public sealed class Finder(
        IUserActivityRepository repository,
        IReportingCalendar calendar,
        IOptions<UserActivityOptions> options) : IService
    {
        private const int WeekDays = 7;

        private const int MonthDays = 30;

        public async Task<UserActivityResponse> Execute(int? requestedDays, Guid? userId, CancellationToken cancellationToken)
        {
            UserActivityOptions settings = options.Value;
            int days = requestedDays ?? settings.DefaultDays;

            if (days < 1 || days > settings.MaxDays)
            {
                throw UserActivityErrors.InvalidDays(settings.MaxDays);
            }

            DateOnly today = calendar.Today();
            DateOnly firstDay = today.AddDays(-(Math.Max(days, MonthDays) - 1));

            IReadOnlyList<UserActivityEntry> entries = await repository.Search(
                new UserActivityBetweenSpecification(calendar.StartOfDayUtc(firstDay), calendar.StartOfDayUtc(today.AddDays(1)), userId),
                cancellationToken);
            List<LocalEntry> local = entries
                .Select(entry => new LocalEntry(calendar.LocalDay(entry.OccurredAt), entry.UserId, entry.Type))
                .ToList();

            List<DailyUserActivityRow> rows = Enumerable.Range(0, days)
                .Select(offset => today.AddDays(offset - (days - 1)))
                .Select(day => ToRow(day, local))
                .ToList();

            return new UserActivityResponse(
                rows,
                ActiveUsersFrom(today, local),
                ActiveUsersFrom(today.AddDays(-(WeekDays - 1)), local),
                ActiveUsersFrom(today.AddDays(-(MonthDays - 1)), local));
        }

        private static DailyUserActivityRow ToRow(DateOnly day, List<LocalEntry> entries)
        {
            List<LocalEntry> ofTheDay = entries.Where(entry => entry.Day == day).ToList();

            return new DailyUserActivityRow(
                day.ToString("yyyy-MM-dd"),
                ofTheDay.Count(entry => entry.Type == UserActivityTypes.Login),
                ofTheDay.Select(entry => entry.UserId).Distinct().Count());
        }

        private static int ActiveUsersFrom(DateOnly firstDay, List<LocalEntry> entries) =>
            entries.Where(entry => entry.Day >= firstDay).Select(entry => entry.UserId).Distinct().Count();

        private sealed record LocalEntry(DateOnly Day, Guid UserId, string Type);
    }
}
