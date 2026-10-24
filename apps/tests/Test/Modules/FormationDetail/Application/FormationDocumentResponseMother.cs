using Nakama.FormationDetail.Application;
using Test.Modules.Shared;

namespace Test.Modules.FormationDetail.Application;

public static class FormationDocumentResponseMother
{
    public static FormationDocumentResponse Random() => With();

    public static FormationDocumentResponse With(Guid? id = null, string? name = null, string? fileName = null) =>
        new(
            id ?? Guid.NewGuid(),
            name ?? WordMother.Random(),
            WordMother.Random(),
            DateTime.UtcNow,
            fileName ?? $"{Guid.NewGuid():N}.pdf");
}
