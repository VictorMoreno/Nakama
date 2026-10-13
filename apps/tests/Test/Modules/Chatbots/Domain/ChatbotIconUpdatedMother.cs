using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotIconUpdatedMother
{
    public static ChatbotIconUpdated Random() => With();

    public static ChatbotIconUpdated Cleared(ChatbotId? id = null) => new(id ?? ChatbotMother.RandomId(), null);

    public static ChatbotIconUpdated With(ChatbotId? id = null, string? iconUrl = null) =>
        new(id ?? ChatbotMother.RandomId(), iconUrl ?? $"{WordMother.Random()}.png");
}
