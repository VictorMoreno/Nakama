using Nakama.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Domain;

public static class FormationDocumentMother
{
    public static FormationDocument Random() => With();

    public static FormationDocument With(
        Guid? id = null,
        string? name = null,
        string? origin = null,
        DateTime? updated = null,
        string? fileName = null) =>
        new(
            id ?? Guid.NewGuid(),
            name ?? WordMother.Random(),
            origin ?? WordMother.Random(),
            updated ?? DateTime.UtcNow,
            fileName ?? $"{Guid.NewGuid():N}.pdf");
}
