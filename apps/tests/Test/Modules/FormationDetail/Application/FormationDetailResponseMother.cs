using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Domain;
using Test.Modules.Shared;

namespace Test.Modules.FormationDetail.Application;

public static class FormationDetailResponseMother
{
    public static FormationDetailResponse Random() => With();

    public static FormationDetailResponse With(
        Guid? id = null,
        string? title = null,
        string? audience = null,
        string? description = null,
        string? status = null,
        bool? archived = null,
        string? iconUrl = null,
        Guid? linkedChatbotKey = null,
        string? linkedChatbotLabel = null,
        List<FormationModuleRow>? modules = null,
        List<FormationMemberRow>? team = null) =>
        new(
            id ?? Guid.NewGuid(),
            title ?? WordMother.Random(),
            audience ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? "Active",
            archived ?? false,
            iconUrl,
            linkedChatbotKey,
            linkedChatbotLabel,
            modules ?? [],
            team ?? []);
}
