using Nakama.Chatbots.Application.DocumentRemover;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotDocumentRemoverCommandMother
{
    public static ChatbotDocumentRemover.Command Random() => With();

    public static ChatbotDocumentRemover.Command With(Guid? id = null, Guid? documentId = null) =>
        new(id ?? ChatbotMother.RandomId().Value, documentId ?? Guid.NewGuid());
}
