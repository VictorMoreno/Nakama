using Nakama.Formations.Application.Unarchiver;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

public static class FormationUnarchiverCommandMother
{
    public static FormationUnarchiver.Command Random() => With();

    public static FormationUnarchiver.Command With(Guid? id = null) => new(id ?? FormationMother.RandomId().Value);
}
