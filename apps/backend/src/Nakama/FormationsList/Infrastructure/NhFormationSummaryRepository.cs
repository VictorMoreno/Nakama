using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using NHibernate.Linq;

namespace Nakama.FormationsList.Infrastructure;

public sealed class NhFormationSummaryRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<FormationSummary, Guid>(readSessions), IFormationSummaryRepository
{
    public Task<IReadOnlyCollection<FormationSummary>> FindAll(CancellationToken ct) => this.FindAllEntities(ct);

    public async Task<IReadOnlyCollection<FormationSummary>> FindByIds(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        using var session = this.Sessions.OpenSession();
        return await session.Query<FormationSummary>().Where(f => ids.Contains(f.Id)).ToListAsync(ct);
    }
}
