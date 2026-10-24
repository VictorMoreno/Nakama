using Nakama.Chatbots.Application.Unarchiver;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotUnarchiverCommandMother
{
    public static ChatbotUnarchiver.Command Random() => With();

    public static ChatbotUnarchiver.Command With(Guid? id = null) => new(id ?? ChatbotMother.RandomId().Value);
}
