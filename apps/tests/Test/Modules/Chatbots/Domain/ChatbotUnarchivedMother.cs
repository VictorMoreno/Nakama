using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotUnarchivedMother
{
    public static ChatbotUnarchived Random() => With();

    public static ChatbotUnarchived With(ChatbotId? id = null) => new(id ?? ChatbotMother.RandomId());
}
