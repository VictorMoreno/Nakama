using Nakama.Formations.Application.ModuleRemover;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

public static class FormationModuleRemoverCommandMother
{
    public static FormationModuleRemover.Command Random() => With();

    public static FormationModuleRemover.Command With(Guid? id = null, Guid? moduleId = null) =>
        new(id ?? FormationMother.RandomId().Value, moduleId ?? FormationModuleMother.RandomId().Value);
}
