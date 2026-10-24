using Nakama.FormationDetail.Domain;
using Test.Modules.Shared;

namespace Test.Modules.FormationDetail.Domain;

public static class FormationDocumentRowMother
{
    public static FormationDocumentRow Random() => With();

    public static FormationDocumentRow With(
        Guid? id = null,
        string? name = null,
        string? origin = null,
        DateTime? updated = null,
        string? fileName = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? WordMother.Random(),
            Origin = origin ?? WordMother.Random(),
            Updated = updated ?? DateTime.UtcNow,
            FileName = fileName ?? $"{Guid.NewGuid():N}.pdf"
        };
}
