using Nakama.UserActivity.Domain;
using Test.Modules.Shared;

namespace Test.Modules.UserActivity.Domain;

public static class UserActivityEntryMother
{
    public static UserActivityEntry Random() => With();

    public static UserActivityEntry With(
        Guid? userId = null,
        string? type = null,
        DateTime? occurredAt = null) =>
        UserActivityEntry.Create(
            Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            type ?? MotherCreator.Random.PickRandom(UserActivityTypes.Login, UserActivityTypes.Session),
            occurredAt ?? DateTime.UtcNow);
}
