using Nakama.FormationEnrollment.Application.Finder;

namespace Test.Modules.FormationEnrollment.Application;

public static class FormationEnrollmentFinderQueryMother
{
    public static FormationEnrollmentFinder.Query Random() => With();

    public static FormationEnrollmentFinder.Query With(Guid? formationId = null, Guid? userId = null) =>
        new(formationId ?? Guid.NewGuid(), userId ?? Guid.NewGuid());
}
