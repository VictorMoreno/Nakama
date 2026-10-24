using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;

namespace Test.Modules.Formations.Domain;

public static class FormationArchivedMother
{
    public static FormationArchived Random() => With();

    public static FormationArchived With(
        FormationId? id = null) =>
        new(
            id ?? FormationMother.RandomId());
}
