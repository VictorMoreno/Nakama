using Nakama.Shared.Infrastructure.UserActivity;

namespace Test.Modules.Shared;

public static class UserActivityOptionsMother
{
    public static UserActivityOptions Random() => With();

    public static UserActivityOptions With(
        int? defaultDays = null,
        int? maxDays = null,
        int? sessionIntervalMinutes = null,
        string? timeZone = null) =>
        new()
        {
            DefaultDays = defaultDays ?? 30,
            MaxDays = maxDays ?? 90,
            SessionIntervalMinutes = sessionIntervalMinutes ?? 60,
            TimeZone = timeZone ?? "Europe/Madrid"
        };
}
