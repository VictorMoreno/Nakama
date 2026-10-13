using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotDocumentAddedMother
{
    public static ChatbotDocumentAdded Random() => With();

    public static ChatbotDocumentAdded With(ChatbotId? id = null, ChatbotDocument? document = null) =>
        new(id ?? ChatbotMother.RandomId(), document ?? ChatbotDocumentMother.Random());
}
