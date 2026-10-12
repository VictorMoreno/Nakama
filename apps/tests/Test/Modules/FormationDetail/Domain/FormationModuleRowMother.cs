using Nakama.FormationDetail.Domain;
using Test.Modules.Shared;

namespace Test.Modules.FormationDetail.Domain;

public static class FormationModuleRowMother
{
    public static FormationModuleRow Random() => With();

    public static FormationModuleRow With(
        Guid? id = null,
        string? title = null,
        string? type = null,
        string? summary = null,
        bool? gated = null,
        bool? aiDraft = null,
        string? resourceUrl = null,
        int? durationMin = null,
        List<QuizQuestionRow>? quiz = null,
        int? passingScore = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Title = title ?? WordMother.Random(),
            Type = type ?? "Document",
            Summary = summary ?? WordMother.Random(),
            Gated = gated ?? false,
            AiDraft = aiDraft ?? false,
            ResourceUrl = resourceUrl,
            DurationMin = durationMin,
            Quiz = quiz,
            PassingScore = passingScore
        };
}
