using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ConversationStartedMother
{
    public static ConversationStarted Random() => With();

    public static ConversationStarted With(
        ConversationId? id = null,
        ChatbotId? botKey = null,
        Guid? userId = null,
        string? title = null,
        DateTime? startedAt = null) =>
        new(
            id ?? ConversationMother.RandomId(),
            botKey ?? ChatbotMother.RandomId(),
            userId ?? Guid.NewGuid(),
            title ?? WordMother.Random(),
            startedAt ?? DateTime.UtcNow);
}
