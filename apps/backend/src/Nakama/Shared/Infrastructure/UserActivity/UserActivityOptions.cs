using Nakama.Shared.Infrastructure.Reporting;

namespace Nakama.Shared.Infrastructure.UserActivity;

public sealed class UserActivityOptions
{
    public const string SectionName = ReportingOptions.SectionName + ":UserActivity";

    public required int DefaultDays { get; init; }

    public required int MaxDays { get; init; }

    public required int SessionIntervalMinutes { get; init; }
}
