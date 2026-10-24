using Nakama.Formations.Application.DocumentAdder;
using Test.Modules.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Application;

public static class FormationDocumentAdderCommandMother
{
    public static FormationDocumentAdder.Command Random() => With();

    public static FormationDocumentAdder.Command With(
        Guid? id = null,
        Guid? documentId = null,
        string? name = null,
        string? origin = null,
        string? originalFileName = null,
        byte[]? content = null)
    {
        byte[] bytes = content ?? MotherCreator.Random.Random.Bytes(16);

        return new FormationDocumentAdder.Command(
            id ?? FormationMother.RandomId().Value,
            documentId ?? Guid.NewGuid(),
            name ?? WordMother.Random(),
            origin ?? WordMother.Random(),
            originalFileName ?? $"{WordMother.Random()}.pdf",
            new MemoryStream(bytes),
            bytes.Length);
    }
}
