namespace Nakama.Formations.Domain;

/// <summary>
/// Current learner's progress in a formation. Embedded in the aggregate for this scope (the
/// frontend models a single demo learner) - see agents.md/plan note: a real multi-user system
/// should promote this to its own <c>LearnerFormationProgress</c> aggregate keyed by
/// userId+formationKey, to avoid serializing unrelated learners' writes against the same lock.
/// Immutable: <see cref="WithModuleCompleted"/> returns a new instance rather than mutating.
/// </summary>
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
