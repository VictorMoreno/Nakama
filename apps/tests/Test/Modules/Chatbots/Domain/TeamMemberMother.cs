using Nakama.Chatbots.Domain;

namespace Test.Modules.Chatbots.Domain;

public static class TeamMemberMother
{
    public static TeamMember Random() => With();

    public static TeamMember With(Guid? userId = null, TeamRole? role = null) =>
        new(userId ?? Guid.NewGuid(), role ?? TeamRole.Editor);
}
