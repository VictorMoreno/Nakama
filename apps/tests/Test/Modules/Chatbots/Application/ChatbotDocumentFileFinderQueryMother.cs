using Nakama.Chatbots.Application.DocumentFileFinder;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotDocumentFileFinderQueryMother
{
    public static ChatbotDocumentFileFinder.Query Random() => With();

    public static ChatbotDocumentFileFinder.Query With(Guid? chatbotId = null, Guid? documentId = null) =>
        new(chatbotId ?? ChatbotMother.RandomId().Value, documentId ?? Guid.NewGuid());
}
