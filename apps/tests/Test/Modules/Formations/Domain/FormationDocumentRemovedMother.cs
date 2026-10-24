using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;

namespace Test.Modules.Formations.Domain;

public static class FormationDocumentRemovedMother
{
    public static FormationDocumentRemoved Random() => With();

    public static FormationDocumentRemoved With(FormationId? id = null, Guid? documentId = null) =>
        new(id ?? FormationMother.RandomId(), documentId ?? Guid.NewGuid());
}
