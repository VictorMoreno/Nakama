using Nakama.Formations.Application.ModuleGatingToggler;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

public static class FormationModuleGatingTogglerCommandMother
{
    public static FormationModuleGatingToggler.Command Random() => With();

    public static FormationModuleGatingToggler.Command With(Guid? id = null, Guid? moduleId = null) =>
        new(id ?? FormationMother.RandomId().Value, moduleId ?? FormationModuleMother.RandomId().Value);
}
