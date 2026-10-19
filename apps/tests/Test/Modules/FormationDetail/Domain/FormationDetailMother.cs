using Nakama.FormationDetail.Domain;
using Test.Modules.Shared;
using FormationDetailEntity = Nakama.FormationDetail.Domain.FormationDetail;

namespace Test.Modules.FormationDetail.Domain;

public static class FormationDetailMother
{
    public static FormationDetailEntity Create(
        Guid id,
        string title,
        string audience,
        string description,
        string status,
        bool archived,
        string? iconUrl,
        List<FormationModuleRow> modules,
        List<FormationMemberRow> team) =>
        new(id, title, audience, description, status, archived, iconUrl, modules, team);

    public static FormationDetailEntity Random() => With();

    public static FormationDetailEntity Populated() =>
        With(
            modules: [FormationModuleRowMother.Random()],
            team: [FormationMemberRowMother.Random()]);

    public static FormationDetailEntity With(
        Guid? id = null,
        string? title = null,
        string? audience = null,
        string? description = null,
        string? status = null,
        bool? archived = null,
        string? iconUrl = null,
        List<FormationModuleRow>? modules = null,
        List<FormationMemberRow>? team = null) =>
        Create(
            id ?? Guid.NewGuid(),
            title ?? WordMother.Random(),
            audience ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? "Active",
            archived ?? false,
            iconUrl,
            modules ?? [],
            team ?? []);
}
