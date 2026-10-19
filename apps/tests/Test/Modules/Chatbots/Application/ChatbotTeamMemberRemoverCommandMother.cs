using Nakama.Chatbots.Application.TeamMemberRemover;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotTeamMemberRemoverCommandMother
{
    public static ChatbotTeamMemberRemover.Command Random() => With();

    public static ChatbotTeamMemberRemover.Command With(Guid? id = null, Guid? userId = null) =>
        new(id ?? ChatbotMother.RandomId().Value, userId ?? Guid.NewGuid());
}
