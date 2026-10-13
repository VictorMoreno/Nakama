using Nakama.LlmProviders.Application.Registerer;
using Nakama.LlmProviders.Domain;
using Test.Modules.Shared;

namespace Test.Modules.LlmProviders.Application;

public static class LlmProviderRegistererCommandMother
{
    public static LlmProviderRegisterer.Command With(
        LlmProvider? provider = null,
        string? apiKey = null,
        bool? enabled = null,
        IReadOnlyList<LlmProviderRegisterer.ModelInput>? models = null) =>
        new(
            (int)(provider ?? MotherCreator.Random.PickRandom<LlmProvider>()),
            apiKey ?? WordMother.Random(),
            enabled ?? true,
            models ?? [new LlmProviderRegisterer.ModelInput(WordMother.Random(), WordMother.Random())]);
}
