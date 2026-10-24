using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;

namespace Test.Modules.Formations.Domain;

public static class FormationDocumentAddedMother
{
    public static FormationDocumentAdded Random() => With();

    public static FormationDocumentAdded With(FormationId? id = null, FormationDocument? document = null) =>
        new(id ?? FormationMother.RandomId(), document ?? FormationDocumentMother.Random());
}
