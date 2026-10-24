using Nakama.Formations.Application.DocumentRemover;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

public static class FormationDocumentRemoverCommandMother
{
    public static FormationDocumentRemover.Command Random() => With();

    public static FormationDocumentRemover.Command With(Guid? id = null, Guid? documentId = null) =>
        new(id ?? FormationMother.RandomId().Value, documentId ?? Guid.NewGuid());
}
