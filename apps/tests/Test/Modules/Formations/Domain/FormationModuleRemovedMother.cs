using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Domain;

public static class FormationModuleRemovedMother
{
    public static FormationModuleRemoved Random() => With();

    public static FormationModuleRemoved With(
        FormationId? id = null,
        ModuleId? moduleId = null) =>
        new(
            id ?? FormationMother.RandomId(),
            moduleId ?? FormationModuleMother.RandomId());
}
