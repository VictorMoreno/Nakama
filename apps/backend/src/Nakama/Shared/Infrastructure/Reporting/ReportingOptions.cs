namespace Nakama.Shared.Infrastructure.Reporting;

public sealed class ReportingOptions
{
    public const string SectionName = "Reporting";

    public required string TimeZone { get; init; }
}
