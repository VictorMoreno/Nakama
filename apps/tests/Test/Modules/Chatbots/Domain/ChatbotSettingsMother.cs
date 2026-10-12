using Nakama.Chatbots.Domain;
using Nakama.LlmProviders.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotSettingsMother
{
    public static ChatbotSettings Random() => With();

    public static ChatbotSettings With(
        bool? socratic = null,
        bool? explicaMas = null,
        LlmProvider? provider = null,
        string? model = null) =>
        new(
            socratic ?? MotherCreator.Random.Random.Bool(),
            explicaMas ?? MotherCreator.Random.Random.Bool(),
            provider ?? MotherCreator.Random.PickRandom<LlmProvider>(),
            model ?? WordMother.Random());
}
