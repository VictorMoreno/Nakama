namespace Nakama.FormationEnrollment.Domain;

public sealed record LearnerProgress(
    IReadOnlyList<Guid> CompletedModuleIds,
    IReadOnlyDictionary<Guid, int> ExamScores,
    IReadOnlyList<ExamAttempt>? ExamAttempts = null)
{
    public IReadOnlyList<ExamAttempt> ExamAttempts { get; init; } = ExamAttempts ?? [];

    public static LearnerProgress Empty { get; } = new([], new Dictionary<Guid, int>(), []);

    public LearnerProgress WithExamAttempt(ExamAttempt attempt)
    {
        LearnerProgress withAttempt = this with { ExamAttempts = [.. this.ExamAttempts, attempt] };

        return attempt.Passed ? withAttempt.WithModuleCompleted(attempt.ModuleId, attempt.Score) : withAttempt;
    }

    public LearnerProgress WithModuleCompleted(Guid moduleId, int? score)
    {
        IReadOnlyList<Guid> completedModuleIds = this.CompletedModuleIds.Contains(moduleId)
            ? this.CompletedModuleIds
            : [.. this.CompletedModuleIds, moduleId];

        IReadOnlyDictionary<Guid, int> examScores = this.ExamScores;
        if (score is not null)
        {
            examScores = new Dictionary<Guid, int>(this.ExamScores) { [moduleId] = score.Value };
        }

        return this with { CompletedModuleIds = completedModuleIds, ExamScores = examScores };
    }
}
