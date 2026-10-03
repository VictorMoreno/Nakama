using Nakama.LlmProviders.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Application.Searcher;

public static class LlmProviderSearcher
{
    public sealed record Query : IQuery<IReadOnlyList<LlmProviderConfigResponse>>;

    public sealed class Handler(Searcher searcher) : IQueryHandler<Query, IReadOnlyList<LlmProviderConfigResponse>>
    {
        public async Task<IReadOnlyList<LlmProviderConfigResponse>> Handle(Query query, CancellationToken ct)
        {
            var configs = await searcher.Execute(ct);
            return configs.Select(c => (LlmProviderConfigResponse)c).ToList();
        }
    }

    public sealed class Searcher(ILlmProviderConfigRepository repository) : IService
    {
        public Task<IReadOnlyList<LlmProviderConfig>> Execute(CancellationToken cancellationToken) =>
            repository.FindAll(cancellationToken);
    }
}
