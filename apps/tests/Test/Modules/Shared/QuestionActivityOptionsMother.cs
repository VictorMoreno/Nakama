using Nakama.Shared.Infrastructure.QuestionActivity;

namespace Test.Modules.Shared;

public static class QuestionActivityOptionsMother
{
    public static QuestionActivityOptions Random() => With();

    public static QuestionActivityOptions With(
        int? defaultWeeks = null,
        int? maxWeeks = null,
        string? timeZone = null) =>
        new()
        {
            DefaultWeeks = defaultWeeks ?? 8,
            MaxWeeks = maxWeeks ?? 26,
            TimeZone = timeZone ?? "Europe/Madrid"
        };
}
