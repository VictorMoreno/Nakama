using MediatR;
using Nakama.FormationsList.Domain;

namespace Nakama.FormationsList.Application.Searcher;

public static class FormationSummarySearcher
{
    public sealed record Query : IRequest<IReadOnlyCollection<FormationSummaryResponse>>;

    public sealed class Handler(IFormationSummaryRepository formationSummaryRepository)
        : IRequestHandler<Query, IReadOnlyCollection<FormationSummaryResponse>>
    {
        public async Task<IReadOnlyCollection<FormationSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            var formations = await formationSummaryRepository.FindAll(ct);
            return formations.Select(e => (FormationSummaryResponse)e).ToList();
        }
    }
}
