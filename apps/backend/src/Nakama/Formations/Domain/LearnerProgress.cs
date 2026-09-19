namespace Nakama.Formations.Domain;

public sealed record LearnerProgress(IReadOnlyList<string> CompletedModuleIds, IReadOnlyDictionary<string, int> ExamScores)
{
    public static LearnerProgress Empty { get; } = new([], new Dictionary<string, int>());

    public LearnerProgress WithModuleCompleted(string moduleId, int? score)
    {
        var completedModuleIds = this.CompletedModuleIds.Contains(moduleId)
            ? this.CompletedModuleIds
            : [.. this.CompletedModuleIds, moduleId];

        var examScores = this.ExamScores;
        if (score is not null)
        {
            examScores = new Dictionary<string, int>(this.ExamScores) { [moduleId] = score.Value };
        }

        return this with { CompletedModuleIds = completedModuleIds, ExamScores = examScores };
    }
}
