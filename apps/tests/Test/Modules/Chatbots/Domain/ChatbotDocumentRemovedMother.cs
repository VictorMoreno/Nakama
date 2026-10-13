using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotDocumentRemovedMother
{
    public static ChatbotDocumentRemoved Random() => With();

    public static ChatbotDocumentRemoved With(ChatbotId? id = null, int? index = null) =>
        new(id ?? ChatbotMother.RandomId(), index ?? 0);
}
