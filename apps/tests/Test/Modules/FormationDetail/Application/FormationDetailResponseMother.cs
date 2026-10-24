using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Domain;
using Test.Modules.Shared;

namespace Test.Modules.FormationDetail.Application;

public static class FormationDetailResponseMother
{
    public static FormationDetailResponse Random() => With();

    public static FormationDetailResponse WithModule() =>
        With(modules: [FormationModuleResponseMother.Random()]);

    public static FormationDetailResponse WithExam(int passingScore, params int[] correctIndexes) =>
        With(modules: [FormationModuleResponseMother.Exam(passingScore, correctIndexes)]);

    public static FormationDetailResponse WithTwoModules(bool firstGated) =>
        With(modules: [FormationModuleResponseMother.With(gated: firstGated), FormationModuleResponseMother.Random()]);

    public static FormationDetailResponse With(
        Guid? id = null,
        string? title = null,
        string? audience = null,
        string? description = null,
        string? status = null,
        bool? archived = null,
        string? iconUrl = null,
        List<FormationModuleResponse>? modules = null,
        List<FormationMemberRow>? team = null,
        List<FormationDocumentResponse>? docs = null) =>
        new(
            id ?? Guid.NewGuid(),
            title ?? WordMother.Random(),
            audience ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? "Active",
            archived ?? false,
            iconUrl,
            modules ?? [],
            team ?? [],
            docs ?? []);
}
