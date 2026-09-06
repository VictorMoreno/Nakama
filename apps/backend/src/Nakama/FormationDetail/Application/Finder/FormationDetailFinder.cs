using MediatR;
using Nakama.FormationDetail.Domain;
using Nakama.FormationDetail.Domain.Errors;

namespace Nakama.FormationDetail.Application.Finder;

public static class FormationDetailFinder
{
    public sealed record Query(string Key) : IRequest<FormationDetailResponse>;

    public sealed class Handler(IFormationDetailRepository formationDetailRepository) : IRequestHandler<Query, FormationDetailResponse>
    {
        public async Task<FormationDetailResponse> Handle(Query query, CancellationToken ct)
        {
            var formation = await formationDetailRepository.FindByKey(query.Key, ct)
                ?? throw FormationDetailErrors.NotFound(query.Key);
            return (FormationDetailResponse)formation;
        }
    }
}
