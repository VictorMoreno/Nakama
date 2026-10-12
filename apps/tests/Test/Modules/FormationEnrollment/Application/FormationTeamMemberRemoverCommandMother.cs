using Nakama.FormationEnrollment.Application.TeamMemberRemover;

namespace Test.Modules.FormationEnrollment.Application;

public static class FormationTeamMemberRemoverCommandMother
{
    public static FormationTeamMemberRemover.Command Random() => With();

    public static FormationTeamMemberRemover.Command With(Guid? id = null, Guid? userId = null) =>
        new(id ?? Guid.NewGuid(), userId ?? Guid.NewGuid());
}
