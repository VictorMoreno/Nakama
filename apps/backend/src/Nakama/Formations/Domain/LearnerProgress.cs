namespace Nakama.Formations.Domain;

public sealed record LearnerProgress(IReadOnlyList<Guid> CompletedModuleIds, IReadOnlyDictionary<Guid, int> ExamScores)
{
    public static LearnerProgress Empty { get; } = new([], new Dictionary<Guid, int>());

    public LearnerProgress WithModuleCompleted(Guid moduleId, int? score)
    {
        var completedModuleIds = this.CompletedModuleIds.Contains(moduleId)
            ? this.CompletedModuleIds
            : [.. this.CompletedModuleIds, moduleId];

        var examScores = this.ExamScores;
        if (score is not null)
        {
            examScores = new Dictionary<Guid, int>(this.ExamScores) { [moduleId] = score.Value };
        }

        return this with { CompletedModuleIds = completedModuleIds, ExamScores = examScores };
    }
}
