using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Domain;

public static class FormationModuleUpdatedMother
{
    public static FormationModuleUpdated Random() => With();

    public static FormationModuleUpdated With(
        FormationId? id = null,
        FormationModule? module = null) =>
        new(
            id ?? FormationMother.RandomId(),
            module ?? FormationModuleMother.Random());
}
