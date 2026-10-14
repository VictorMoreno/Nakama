using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.LlmProviders.Domain;

public sealed class LlmProviderConfigListingSpecification : BaseSpecification<LlmProviderConfig>
{
    protected override Expression<Func<LlmProviderConfig, bool>> CreateExpression() => PredicateBuilder.New<LlmProviderConfig>();
}
