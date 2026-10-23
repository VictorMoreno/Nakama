using Nakama.Shared.Infrastructure.Reporting;

namespace Test.Modules.Shared;

public static class ReportingOptionsMother
{
    public static ReportingOptions Random() => With();

    public static ReportingOptions With(string? timeZone = null) =>
        new() { TimeZone = timeZone ?? "Europe/Madrid" };
}
