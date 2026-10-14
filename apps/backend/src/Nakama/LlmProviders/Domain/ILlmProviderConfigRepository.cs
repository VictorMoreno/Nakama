using Nakama.Shared.Domain.Specification;

namespace Nakama.LlmProviders.Domain;

public interface ILlmProviderConfigRepository
{
    Task<IReadOnlyList<LlmProviderConfig>> Search(ISpecification<LlmProviderConfig> specification, CancellationToken ct);

    Task Save(LlmProviderConfig config, CancellationToken ct);
}
