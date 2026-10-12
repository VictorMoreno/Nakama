using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotCreatedMother
{
    public static ChatbotCreated Random() => With();

    public static ChatbotCreated With(
        ChatbotId? id = null,
        string? label = null,
        string? description = null,
        ChatbotStatus? status = null,
        ChatbotSettings? settings = null) =>
        new(
            id ?? ChatbotMother.RandomId(),
            label ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? ChatbotStatus.Active,
            settings ?? ChatbotSettings.Default);
}
