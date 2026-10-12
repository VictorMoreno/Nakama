using Nakama.Chatbots.Application.SettingsUpdater;
using Nakama.LlmProviders.Domain;
using Test.Modules.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotSettingsUpdaterCommandMother
{
    public static ChatbotSettingsUpdater.Command Random() => With();

    public static ChatbotSettingsUpdater.Command With(
        Guid? id = null,
        bool? socratic = null,
        bool? explicaMas = null,
        LlmProvider? provider = null,
        string? model = null) =>
        new(
            id ?? ChatbotMother.RandomId().Value,
            socratic ?? MotherCreator.Random.Random.Bool(),
            explicaMas ?? MotherCreator.Random.Random.Bool(),
            (int)(provider ?? MotherCreator.Random.PickRandom<LlmProvider>()),
            model ?? WordMother.Random());
}
