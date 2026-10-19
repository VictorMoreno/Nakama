using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotTeamMemberRemovedMother
{
    public static ChatbotTeamMemberRemoved Random() => With();

    public static ChatbotTeamMemberRemoved With(ChatbotId? id = null, Guid? userId = null) =>
        new(id ?? ChatbotMother.RandomId(), userId ?? Guid.NewGuid());
}
