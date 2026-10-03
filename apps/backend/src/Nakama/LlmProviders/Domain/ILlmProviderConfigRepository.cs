namespace Nakama.LlmProviders.Domain;

public interface ILlmProviderConfigRepository
{
    Task<LlmProviderConfig?> FindByProvider(LlmProvider provider, CancellationToken ct);

    Task<IReadOnlyList<LlmProviderConfig>> FindAll(CancellationToken ct);

    Task Save(LlmProviderConfig config, CancellationToken ct);
}
