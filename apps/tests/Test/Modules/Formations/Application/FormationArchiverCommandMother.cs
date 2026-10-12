using Nakama.Formations.Application.Archiver;
using Test.Modules.Formations.Domain;

namespace Test.Modules.Formations.Application;

public static class FormationArchiverCommandMother
{
    public static FormationArchiver.Command Random() => With();

    public static FormationArchiver.Command With(Guid? id = null) => new(id ?? FormationMother.RandomId().Value);
}
