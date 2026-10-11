using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using NHibernate;
using NHibernate.Linq;

namespace Nakama.FormationsList.Infrastructure;

public sealed class NhFormationSummaryRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<FormationSummary, Guid>(readSessions), IFormationSummaryRepository
{
    public async Task<IReadOnlyCollection<FormationSummary>> FindAll(CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<FormationSummary>().OrderBy(f => f.Title).ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<FormationSummary>> FindByIds(IReadOnlyCollection<Guid> ids, CancellationToken ct)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        using ISession session = this.Sessions.OpenSession();
        return await session.Query<FormationSummary>().Where(f => ids.Contains(f.Id)).OrderBy(f => f.Title).ToListAsync(ct);
    }
}
