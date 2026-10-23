using Nakama.QuestionActivity.Domain;

namespace Test.Modules.QuestionActivity.Domain;

public static class QuestionActivityEntryMother
{
    public static QuestionActivityEntry Random() => With();

    public static QuestionActivityEntry With(
        Guid? botKey = null,
        Guid? userId = null,
        DateTime? occurredAt = null) =>
        QuestionActivityEntry.Create(
            Guid.NewGuid(),
            botKey ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            occurredAt ?? DateTime.UtcNow);
}
