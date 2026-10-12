using Nakama.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ConversationExchangeMother
{
    public static ConversationExchange Random() => With();

    public static ConversationExchange With(
        string? question = null,
        string? answer = null,
        string? citation = null,
        string? moreExplanation = null,
        string? socraticVariant = null,
        IReadOnlyList<ExchangeAttachment>? attachments = null) =>
        new(
            question ?? WordMother.Random(),
            answer ?? WordMother.Random(),
            citation ?? WordMother.Random(),
            moreExplanation ?? WordMother.Random(),
            socraticVariant ?? WordMother.Random(),
            attachments);
}
