using Nakama.FormationEnrollment.Domain;

namespace Test.Modules.FormationEnrollment.Domain;

public static class FormationEnrollmentMother
{
    public static Nakama.FormationEnrollment.Domain.FormationEnrollment Create(
        FormationEnrollmentId id,
        Guid formationId,
        Guid userId,
        LearnerProgress progress) =>
        new(id, formationId, userId, progress);

    public static Nakama.FormationEnrollment.Domain.FormationEnrollment Random() => With();

    public static FormationEnrollmentId RandomId() => FormationEnrollmentId.From(Guid.NewGuid());

    public static Nakama.FormationEnrollment.Domain.FormationEnrollment With(
        FormationEnrollmentId? id = null,
        Guid? formationId = null,
        Guid? userId = null,
        LearnerProgress? progress = null) =>
        Create(
            id ?? RandomId(),
            formationId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            progress ?? LearnerProgressMother.Empty());
}
