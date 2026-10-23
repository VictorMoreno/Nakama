namespace Nakama.Shared.Domain;

public interface IReportingCalendar
{
    DateOnly Today();

    DateOnly LocalDay(DateTime utc);

    DateTime StartOfDayUtc(DateOnly day);

    DateOnly WeekStart(DateOnly day);
}
