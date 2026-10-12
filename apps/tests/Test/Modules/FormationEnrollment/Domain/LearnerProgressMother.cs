using Nakama.FormationEnrollment.Domain;

namespace Test.Modules.FormationEnrollment.Domain;

public static class LearnerProgressMother
{
    public static LearnerProgress Empty() => With();

    public static LearnerProgress With(
        IReadOnlyList<Guid>? completedModuleIds = null,
        IReadOnlyDictionary<Guid, int>? examScores = null) =>
        new(completedModuleIds ?? [], examScores ?? new Dictionary<Guid, int>());
}
