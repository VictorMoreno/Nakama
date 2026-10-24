using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;

namespace Test.Modules.Formations.Domain;

public static class FormationModuleGatingToggledMother
{
    public static FormationModuleGatingToggled Random() => With();

    public static FormationModuleGatingToggled With(
        FormationId? id = null,
        ModuleId? moduleId = null,
        bool? gated = null) =>
        new(
            id ?? FormationMother.RandomId(),
            moduleId ?? FormationModuleMother.RandomId(),
            gated ?? true);
}
