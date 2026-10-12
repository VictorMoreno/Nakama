using Nakama.Chatbots.Application.Remover;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ConversationRemoverCommandMother
{
    public static ConversationRemover.Command Random() => With();

    public static ConversationRemover.Command With(Guid? conversationId = null, Guid? userId = null) =>
        new(conversationId ?? ConversationMother.RandomId().Value, userId ?? Guid.NewGuid());
}
