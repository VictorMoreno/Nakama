using Nakama.FormationEnrollment.Domain.Events;

namespace Test.Modules.FormationEnrollment.Domain;

public static class PersonUnenrolledMother
{
    public static PersonUnenrolled Random() => With();

    public static PersonUnenrolled With(
        Guid? formationId = null,
        Guid? userId = null) =>
        new(
            formationId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid());
}
