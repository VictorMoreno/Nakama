using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ConversationExchangeAppendedMother
{
    public static ConversationExchangeAppended Random() => With();

    public static ConversationExchangeAppended With(
        ConversationId? id = null,
        ChatbotId? botKey = null,
        Guid? userId = null,
        ConversationExchange? exchange = null) =>
        new(
            id ?? ConversationMother.RandomId(),
            botKey ?? ChatbotMother.RandomId(),
            userId ?? Guid.NewGuid(),
            exchange ?? ConversationExchangeMother.Random());
}
