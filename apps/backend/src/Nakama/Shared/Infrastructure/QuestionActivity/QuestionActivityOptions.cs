using Nakama.Shared.Infrastructure.Reporting;

namespace Nakama.Shared.Infrastructure.QuestionActivity;

public sealed class QuestionActivityOptions
{
    public const string SectionName = ReportingOptions.SectionName + ":QuestionActivity";

    public required int DefaultWeeks { get; init; }

    public required int MaxWeeks { get; init; }
}
