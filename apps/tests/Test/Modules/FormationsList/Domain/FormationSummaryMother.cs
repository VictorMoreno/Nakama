using Nakama.FormationsList.Domain;
using Test.Modules.Shared;

namespace Test.Modules.FormationsList.Domain;

public static class FormationSummaryMother
{
    public static FormationSummary Create(
        Guid id,
        string title,
        string audience,
        string description,
        string status,
        bool archived,
        string? iconUrl,
        int modulesCount) =>
        new(id, title, audience, description, status, archived, iconUrl, modulesCount);

    public static FormationSummary Random() => With();

    public static FormationSummary With(
        Guid? id = null,
        string? title = null,
        string? audience = null,
        string? description = null,
        string? status = null,
        bool? archived = null,
        string? iconUrl = null,
        int? modulesCount = null) =>
        Create(
            id ?? Guid.NewGuid(),
            title ?? WordMother.Random(),
            audience ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? "Active",
            archived ?? false,
            iconUrl,
            modulesCount ?? MotherCreator.Random.Random.Number(1, 10));
}
