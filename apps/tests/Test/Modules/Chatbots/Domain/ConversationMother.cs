using Nakama.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ConversationMother
{
    public static Conversation Create(
        ConversationId id,
        ChatbotId botKey,
        Guid userId,
        string title,
        DateTime startedAt,
        IReadOnlyList<ConversationExchange> exchanges) =>
        new(id, botKey, userId, title, startedAt, exchanges);

    public static Conversation Random() => With();

    public static ConversationId RandomId() => ConversationId.From(Guid.NewGuid());

    public static Conversation With(
        ConversationId? id = null,
        ChatbotId? botKey = null,
        Guid? userId = null,
        string? title = null,
        DateTime? startedAt = null,
        IReadOnlyList<ConversationExchange>? exchanges = null) =>
        Create(
            id ?? RandomId(),
            botKey ?? ChatbotId.From(Guid.NewGuid()),
            userId ?? Guid.NewGuid(),
            title ?? WordMother.Random(),
            startedAt ?? DateTime.UtcNow,
            exchanges ?? []);
}
