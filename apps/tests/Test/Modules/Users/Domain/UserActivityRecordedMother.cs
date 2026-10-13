using Nakama.Users.Domain;
using Nakama.Users.Domain.Events;

namespace Test.Modules.Users.Domain;

public static class UserActivityRecordedMother
{
    public static UserActivityRecorded Random() => With();

    public static UserActivityRecorded With(UserId? id = null, DateTime? occurredAt = null) =>
        new(id ?? UserMother.RandomId(), occurredAt ?? DateTime.UtcNow);
}
