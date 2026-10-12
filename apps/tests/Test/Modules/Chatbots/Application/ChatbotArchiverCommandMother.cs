using Nakama.Chatbots.Application.Archiver;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotArchiverCommandMother
{
    public static ChatbotArchiver.Command Random() => With();

    public static ChatbotArchiver.Command With(Guid? id = null) => new(id ?? ChatbotMother.RandomId().Value);
}
