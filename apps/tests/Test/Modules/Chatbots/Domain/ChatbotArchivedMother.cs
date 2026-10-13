using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotArchivedMother
{
    public static ChatbotArchived Random() => With();

    public static ChatbotArchived With(ChatbotId? id = null) => new(id ?? ChatbotMother.RandomId());
}
