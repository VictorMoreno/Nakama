using Nakama.Chatbots.Application.TeamMemberAdder;
using Nakama.Chatbots.Domain;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotTeamMemberAdderCommandMother
{
    public static ChatbotTeamMemberAdder.Command Random() => With();

    public static ChatbotTeamMemberAdder.Command With(Guid? id = null, Guid? userId = null, TeamRole? role = null) =>
        new(
            id ?? ChatbotMother.RandomId().Value,
            userId ?? Guid.NewGuid(),
            (int)(role ?? TeamRole.Editor));
}
