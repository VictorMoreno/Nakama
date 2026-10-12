using Nakama.FormationEnrollment.Application.TeamMemberAdder;

namespace Test.Modules.FormationEnrollment.Application;

public static class FormationTeamMemberAdderCommandMother
{
    public static FormationTeamMemberAdder.Command Random() => With();

    public static FormationTeamMemberAdder.Command With(Guid? id = null, Guid? userId = null) =>
        new(id ?? Guid.NewGuid(), userId ?? Guid.NewGuid());
}
