using Nakama.FormationDetail.Domain;
using Nakama.FormationDetail.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.FormationDetail.Application.Finder;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

public static class FormationDetailFinder
{
    public sealed record Query(Guid Id) : IQuery<FormationDetailResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, FormationDetailResponse>
    {
        public async Task<FormationDetailResponse> Handle(Query query, CancellationToken ct)
        {
            var formation = await finder.Execute(query.Id, ct);
            return (FormationDetailResponse)formation;
        }
    }

    public sealed class Finder(IFormationDetailRepository repository) : IService
    {
        public async Task<FormationDetail> Execute(Guid id, CancellationToken cancellationToken) =>
            await repository.FindById(id, cancellationToken)
                ?? throw FormationDetailErrors.NotFound(id.ToString());
    }
}
