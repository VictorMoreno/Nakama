using Nakama.FormationEnrollment.Application.ModuleCompleter;

namespace Test.Modules.FormationEnrollment.Application;

public static class FormationModuleCompleterCommandMother
{
    public static FormationModuleCompleter.Command Random() => With();

    public static FormationModuleCompleter.Command With(
        Guid? id = null,
        Guid? moduleId = null,
        Guid? userId = null,
        int? score = null) =>
        new(id ?? Guid.NewGuid(), moduleId ?? Guid.NewGuid(), userId ?? Guid.NewGuid(), score);
}
