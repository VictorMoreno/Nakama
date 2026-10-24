using Nakama.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Domain;

public static class FormationMother
{
    public static Formation Create(
        FormationId id,
        string title,
        string audience,
        string description,
        FormationStatus status,
        string? iconUrl,
        bool archived,
        IReadOnlyList<FormationModule> modules,
        IReadOnlyList<FormationDocument> documents) =>
        new(id, title, audience, description, status, iconUrl, archived, modules, documents);

    public static Formation Random() => With();

    public static Formation Archived() => With(archived: true);

    public static FormationId RandomId() => FormationId.From(Guid.NewGuid());

    public static Formation With(
        FormationId? id = null,
        string? title = null,
        string? audience = null,
        string? description = null,
        FormationStatus? status = null,
        string? iconUrl = null,
        bool? archived = null,
        IReadOnlyList<FormationModule>? modules = null,
        IReadOnlyList<FormationDocument>? documents = null) =>
        Create(
            id ?? RandomId(),
            title ?? WordMother.Random(),
            audience ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? FormationStatus.Active,
            iconUrl,
            archived ?? false,
            modules ?? [],
            documents ?? []);
}
