using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotDocumentRemovedMother
{
    public static ChatbotDocumentRemoved Random() => With();

    public static ChatbotDocumentRemoved With(ChatbotId? id = null, Guid? documentId = null) =>
        new(id ?? ChatbotMother.RandomId(), documentId ?? Guid.NewGuid());
}
