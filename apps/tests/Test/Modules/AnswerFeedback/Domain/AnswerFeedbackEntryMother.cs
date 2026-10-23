using Nakama.AnswerFeedback.Domain;
using Test.Modules.Shared;

namespace Test.Modules.AnswerFeedback.Domain;

public static class AnswerFeedbackEntryMother
{
    public static AnswerFeedbackEntry Random() => With();

    public static AnswerFeedbackEntry With(
        Guid? botKey = null,
        string? question = null,
        bool? useful = null,
        DateTime? ratedAt = null) =>
        AnswerFeedbackEntry.Create(
            Guid.NewGuid(),
            0,
            botKey ?? Guid.NewGuid(),
            question ?? WordMother.Random(),
            useful ?? false,
            ratedAt ?? DateTime.UtcNow);
}
