using Nakama.FormationEnrollment.Domain.Events;

namespace Test.Modules.FormationEnrollment.Domain;

public static class EnrollmentModuleCompletedMother
{
    public static EnrollmentModuleCompleted Random() => With();

    public static EnrollmentModuleCompleted With(
        Guid? formationId = null,
        Guid? userId = null,
        Guid? moduleId = null,
        int? score = null,
        int? completedModuleCount = null) =>
        new(
            formationId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            moduleId ?? Guid.NewGuid(),
            score,
            completedModuleCount ?? 1);
}
