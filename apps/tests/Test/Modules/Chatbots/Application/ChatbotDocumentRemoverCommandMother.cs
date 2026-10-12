using Nakama.Chatbots.Application.DocumentRemover;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotDocumentRemoverCommandMother
{
    public static ChatbotDocumentRemover.Command Random() => With();

    public static ChatbotDocumentRemover.Command With(Guid? id = null, int index = 0) =>
        new(id ?? ChatbotMother.RandomId().Value, index);
}
