using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotStatusChangedMother
{
    public static ChatbotStatusChanged Random() => With();

    public static ChatbotStatusChanged With(ChatbotId? id = null, ChatbotStatus? status = null) =>
        new(id ?? ChatbotMother.RandomId(), status ?? ChatbotStatus.Active);
}
