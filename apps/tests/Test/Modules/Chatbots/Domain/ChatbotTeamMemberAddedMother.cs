using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotTeamMemberAddedMother
{
    public static ChatbotTeamMemberAdded Random() => With();

    public static ChatbotTeamMemberAdded With(ChatbotId? id = null, TeamMember? member = null) =>
        new(id ?? ChatbotMother.RandomId(), member ?? TeamMemberMother.Random());
}
