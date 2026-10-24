namespace Nakama.FormationEnrollment.Application;

using FormationEnrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

public sealed record FormationEnrollmentResponse(
    Guid FormationId,
    Guid UserId,
    IReadOnlyList<Guid> CompletedModuleIds,
    IReadOnlyDictionary<Guid, int> ExamScores,
    IReadOnlyList<ExamAttemptResponse> ExamAttempts)
{
    public static explicit operator FormationEnrollmentResponse(FormationEnrollment entity) => new(
        entity.FormationId,
        entity.UserId,
        entity.Progress.CompletedModuleIds,
        entity.Progress.ExamScores,
        entity.Progress.ExamAttempts
            .Select(attempt => new ExamAttemptResponse(attempt.ModuleId, attempt.Score, attempt.Passed, attempt.AttemptedAt))
            .ToList());
}

public sealed record ExamAttemptResponse(Guid ModuleId, int Score, bool Passed, DateTime AttemptedAt);
