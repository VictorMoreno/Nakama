using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotDetailsUpdatedMother
{
    public static ChatbotDetailsUpdated Random() => With();

    public static ChatbotDetailsUpdated With(ChatbotId? id = null, string? label = null, string? description = null) =>
        new(id ?? ChatbotMother.RandomId(), label ?? WordMother.Random(), description ?? WordMother.Random());
}
