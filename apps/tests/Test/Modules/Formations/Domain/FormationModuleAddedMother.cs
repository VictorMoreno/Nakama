using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;

namespace Test.Modules.Formations.Domain;

public static class FormationModuleAddedMother
{
    public static FormationModuleAdded Random() => With();

    public static FormationModuleAdded With(
        FormationId? id = null,
        FormationModule? module = null) =>
        new(
            id ?? FormationMother.RandomId(),
            module ?? FormationModuleMother.Random());
}
