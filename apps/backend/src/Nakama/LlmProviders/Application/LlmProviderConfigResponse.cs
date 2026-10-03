namespace Nakama.LlmProviders.Application;

using LlmProviderConfig = Nakama.LlmProviders.Domain.LlmProviderConfig;

public sealed record LlmProviderConfigResponse(
    int Provider,
    bool Configured,
    bool Enabled,
    IReadOnlyList<LlmModelOptionResponse> Models,
    DateTime UpdatedAt)
{
    public static explicit operator LlmProviderConfigResponse(LlmProviderConfig entity) => new(
        (int)entity.Provider,
        !string.IsNullOrEmpty(entity.ApiKey),
        entity.Enabled,
        entity.Models.Select(m => new LlmModelOptionResponse(m.Id, m.Label)).ToList(),
        entity.UpdatedAt);
}

public sealed record LlmModelOptionResponse(string Id, string Label);
