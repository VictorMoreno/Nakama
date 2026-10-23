using Microsoft.Extensions.Options;
using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Reporting;

public sealed class ReportingCalendar(ISystemClock systemClock, IOptions<ReportingOptions> options) : IReportingCalendar
{
    private const int DaysPerWeek = 7;

    private readonly TimeZoneInfo _zone = TimeZoneInfo.FindSystemTimeZoneById(options.Value.TimeZone);

    public DateOnly Today() => this.LocalDay(systemClock.UtcNow);

    public DateOnly LocalDay(DateTime utc) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), this._zone));

    public DateTime StartOfDayUtc(DateOnly day) =>
        TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(TimeOnly.MinValue), this._zone);

    public DateOnly WeekStart(DateOnly day) => day.AddDays(-(((int)day.DayOfWeek + 6) % DaysPerWeek));
}
