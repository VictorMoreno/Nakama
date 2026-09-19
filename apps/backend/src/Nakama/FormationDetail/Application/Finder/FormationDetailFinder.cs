using Nakama.FormationDetail.Domain;
using Nakama.FormationDetail.Domain.Errors;
using Nakama.Shared.Domain;

namespace Nakama.FormationDetail.Application.Finder;

public static class FormationDetailFinder
{
    public sealed record Query(Guid Id) : IQuery<FormationDetailResponse>;

    public sealed class Handler(IFormationDetailRepository formationDetailRepository) : IQueryHandler<Query, FormationDetailResponse>
    {
        public async Task<FormationDetailResponse> Handle(Query query, CancellationToken ct)
        {
            var formation = await formationDetailRepository.FindById(query.Id, ct)
                ?? throw FormationDetailErrors.NotFound(query.Id.ToString());
            return (FormationDetailResponse)formation;
        }
    }
}
