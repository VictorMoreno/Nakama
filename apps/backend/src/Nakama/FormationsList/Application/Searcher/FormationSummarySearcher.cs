using Nakama.FormationsList.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.FormationsList.Application.Searcher;

public static class FormationSummarySearcher
{
    public sealed record Query : IQuery<IReadOnlyCollection<FormationSummaryResponse>>;

    public sealed class Handler(Searcher searcher)
        : IQueryHandler<Query, IReadOnlyCollection<FormationSummaryResponse>>
    {
        public async Task<IReadOnlyCollection<FormationSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            var formations = await searcher.Execute(ct);
            return formations.Select(e => (FormationSummaryResponse)e).ToList();
        }
    }

    public sealed class Searcher(IFormationSummaryRepository repository) : IService
    {
        public Task<IReadOnlyCollection<FormationSummary>> Execute(CancellationToken cancellationToken) =>
            repository.FindAll(cancellationToken);
    }
}
