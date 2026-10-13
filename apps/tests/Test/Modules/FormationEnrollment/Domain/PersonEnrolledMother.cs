using Nakama.FormationEnrollment.Domain.Events;

namespace Test.Modules.FormationEnrollment.Domain;

public static class PersonEnrolledMother
{
    public static PersonEnrolled Random() => With();

    public static PersonEnrolled With(
        Guid? formationId = null,
        Guid? userId = null) =>
        new(
            formationId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid());
}
