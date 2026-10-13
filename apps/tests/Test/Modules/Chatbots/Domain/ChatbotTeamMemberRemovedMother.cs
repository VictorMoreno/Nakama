using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotTeamMemberRemovedMother
{
    public static ChatbotTeamMemberRemoved Random() => With();

    public static ChatbotTeamMemberRemoved With(ChatbotId? id = null, int? index = null) =>
        new(id ?? ChatbotMother.RandomId(), index ?? 0);
}
