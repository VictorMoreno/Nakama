namespace Nakama.FormationEnrollment.Application;

using FormationEnrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

public sealed record FormationEnrollmentResponse(
    Guid FormationId,
    Guid UserId,
    IReadOnlyList<Guid> CompletedModuleIds,
    IReadOnlyDictionary<Guid, int> ExamScores)
{
    public static explicit operator FormationEnrollmentResponse(FormationEnrollment entity) => new(
        entity.FormationId,
        entity.UserId,
        entity.Progress.CompletedModuleIds,
        entity.Progress.ExamScores);
}
