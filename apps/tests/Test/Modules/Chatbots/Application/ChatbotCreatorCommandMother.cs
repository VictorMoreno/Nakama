using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Domain;
using Nakama.LlmProviders.Domain;
using Test.Modules.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotCreatorCommandMother
{
    public static ChatbotCreator.Command Random() => With();

    public static ChatbotCreator.Command With(
        Guid? id = null,
        string? label = null,
        string? description = null,
        ChatbotStatus? status = null,
        bool? socratic = null,
        bool? explicaMas = null,
        LlmProvider? provider = null,
        string? model = null) =>
        new(
            id ?? ChatbotMother.RandomId().Value,
            label ?? WordMother.Random(),
            description ?? WordMother.Random(),
            (int)(status ?? ChatbotStatus.Active),
            socratic ?? MotherCreator.Random.Random.Bool(),
            explicaMas ?? MotherCreator.Random.Random.Bool(),
            (int)(provider ?? MotherCreator.Random.PickRandom<LlmProvider>()),
            model ?? WordMother.Random());
}
