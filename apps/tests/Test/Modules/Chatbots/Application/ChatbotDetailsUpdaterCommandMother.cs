using Nakama.Chatbots.Application.DetailsUpdater;
using Test.Modules.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotDetailsUpdaterCommandMother
{
    public static ChatbotDetailsUpdater.Command Random() => With();

    public static ChatbotDetailsUpdater.Command With(Guid? id = null, string? label = null, string? description = null) =>
        new(id ?? ChatbotMother.RandomId().Value, label ?? WordMother.Random(), description ?? WordMother.Random());
}
