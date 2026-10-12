using Nakama.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Domain;

public static class FormationModuleMother
{
    public static FormationModule Random() => With();

    public static ModuleId RandomId() => ModuleId.From(Guid.NewGuid());

    public static FormationModule With(
        ModuleId? id = null,
        string? title = null,
        ModuleType? type = null,
        string? summary = null,
        bool? gated = null,
        bool? aiDraft = null,
        string? resourceUrl = null,
        int? durationMin = null,
        IReadOnlyList<QuizQuestion>? quiz = null,
        int? passingScore = null) =>
        new(
            id ?? RandomId(),
            title ?? WordMother.Random(),
            type ?? ModuleType.Document,
            summary ?? WordMother.Random(),
            gated ?? false,
            aiDraft ?? false,
            resourceUrl,
            durationMin,
            quiz,
            passingScore);
}
