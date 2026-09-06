using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.FormationDetail.Infrastructure;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

public sealed class NhFormationDetailRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<FormationDetail, string>(readSessions), IFormationDetailRepository
{
    public Task<FormationDetail?> FindByKey(string key, CancellationToken ct) => this.FindById(key, ct);
}
