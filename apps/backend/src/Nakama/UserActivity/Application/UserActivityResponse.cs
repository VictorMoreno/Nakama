namespace Nakama.UserActivity.Application;

public sealed record DailyUserActivityRow(string Date, int Logins, int ActiveUsers);

public sealed record UserActivityResponse(
    IReadOnlyList<DailyUserActivityRow> Days,
    int ActiveToday,
    int ActiveLast7Days,
    int ActiveLast30Days);
