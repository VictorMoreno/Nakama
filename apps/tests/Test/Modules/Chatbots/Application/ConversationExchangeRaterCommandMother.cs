using Nakama.Chatbots.Application.ExchangeRater;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ConversationExchangeRaterCommandMother
{
    public static ConversationExchangeRater.Command With(
        Guid? botKey = null,
        Guid? conversationId = null,
        int exchangeIndex = 0,
        bool useful = true,
        Guid? userId = null) =>
        new(
            botKey ?? Guid.NewGuid(),
            conversationId ?? ConversationMother.RandomId().Value,
            exchangeIndex,
            useful,
            userId ?? Guid.NewGuid());
}
