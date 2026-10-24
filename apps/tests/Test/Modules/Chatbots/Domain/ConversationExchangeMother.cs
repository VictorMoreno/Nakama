using Nakama.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ConversationExchangeMother
{
    public static ConversationExchange Random() => With();

    public static ConversationExchange With(
        string? question = null,
        string? answer = null,
        IReadOnlyList<ExchangeAttachment>? attachments = null,
        bool? useful = null) =>
        new(
            question ?? WordMother.Random(),
            answer ?? WordMother.Random(),
            attachments,
            useful);
}
