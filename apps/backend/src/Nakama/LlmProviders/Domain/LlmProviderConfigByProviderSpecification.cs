using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.LlmProviders.Domain;

public sealed class LlmProviderConfigByProviderSpecification(LlmProvider provider) : BaseSpecification<LlmProviderConfig>
{
    public LlmProvider Provider { get; } = provider;

    protected override Expression<Func<LlmProviderConfig, bool>> CreateExpression() =>
        config => config.Provider == this.Provider;
}
