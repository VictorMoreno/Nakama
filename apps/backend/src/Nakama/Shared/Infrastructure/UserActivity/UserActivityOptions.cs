namespace Nakama.Shared.Infrastructure.UserActivity;

public sealed class UserActivityOptions
{
    public const string SectionName = "UserActivity";

    public required int DefaultDays { get; init; }

    public required int MaxDays { get; init; }

    public required int SessionIntervalMinutes { get; init; }

    public required string TimeZone { get; init; }
}
