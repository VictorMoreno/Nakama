using Nakama.LlmProviders.Domain;
using Test.Modules.Shared;

namespace Test.Modules.LlmProviders.Domain;

public static class LlmProviderConfigMother
{
    public static LlmProviderConfig Create(
        LlmProviderConfigId id,
        LlmProvider provider,
        string apiKey,
        bool enabled,
        IReadOnlyList<LlmModelOption> models,
        DateTime updatedAt) =>
        new(id, provider, apiKey, enabled, models, updatedAt);

    public static LlmProviderConfig Random() => With();

    public static LlmProviderConfigId RandomId() => LlmProviderConfigId.From(Guid.NewGuid());

    public static LlmProviderConfig With(
        LlmProviderConfigId? id = null,
        LlmProvider? provider = null,
        string? apiKey = null,
        bool? enabled = null,
        IReadOnlyList<LlmModelOption>? models = null,
        DateTime? updatedAt = null) =>
        Create(
            id ?? RandomId(),
            provider ?? MotherCreator.Random.PickRandom<LlmProvider>(),
            apiKey ?? WordMother.Random(),
            enabled ?? true,
            models ?? [LlmModelOptionMother.Random()],
            updatedAt ?? DateTime.UtcNow.AddDays(-1));
}
