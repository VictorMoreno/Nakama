using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;

namespace Test.Modules.Formations.Domain;

public static class FormationUnarchivedMother
{
    public static FormationUnarchived Random() => With();

    public static FormationUnarchived With(
        FormationId? id = null) =>
        new(
            id ?? FormationMother.RandomId());
}
