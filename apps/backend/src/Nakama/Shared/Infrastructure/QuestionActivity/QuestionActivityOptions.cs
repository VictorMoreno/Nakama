namespace Nakama.Shared.Infrastructure.QuestionActivity;

public sealed class QuestionActivityOptions
{
    public const string SectionName = "QuestionActivity";

    public required int DefaultWeeks { get; init; }

    public required int MaxWeeks { get; init; }

    public required string TimeZone { get; init; }
}
