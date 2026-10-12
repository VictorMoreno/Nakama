using Nakama.Formations.Application.ModuleUpdater;
using Nakama.Formations.Domain;
using Test.Modules.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Application;

public static class FormationModuleUpdaterCommandMother
{
    public static FormationModuleUpdater.Command Random() => With();

    public static FormationModuleUpdater.Command With(
        Guid? id = null,
        Guid? moduleId = null,
        string? title = null,
        ModuleType? type = null,
        string? summary = null,
        bool? gated = null,
        bool? aiDraft = null,
        string? resourceUrl = null,
        int? durationMin = null,
        IReadOnlyList<FormationModuleUpdater.QuizQuestionInput>? quiz = null,
        int? passingScore = null) =>
        new(
            id ?? FormationMother.RandomId().Value,
            moduleId ?? FormationModuleMother.RandomId().Value,
            title ?? WordMother.Random(),
            (int)(type ?? ModuleType.Document),
            summary ?? WordMother.Random(),
            gated ?? false,
            aiDraft ?? false,
            resourceUrl,
            durationMin,
            quiz,
            passingScore);
}
