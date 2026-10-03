using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Domain.Events;

public sealed record LlmProviderConfigured(
    LlmProviderConfigId Id,
    LlmProvider Provider,
    bool Enabled,
    IReadOnlyList<LlmModelOption> Models) : IDomainEvent;
