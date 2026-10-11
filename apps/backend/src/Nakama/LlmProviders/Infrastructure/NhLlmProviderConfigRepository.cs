using Nakama.LlmProviders.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using NHibernate;
using NHibernate.Linq;

namespace Nakama.LlmProviders.Infrastructure;

public sealed class NhLlmProviderConfigRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<LlmProviderConfig, LlmProviderConfigId>(writeSessions), ILlmProviderConfigRepository
{
    public async Task<LlmProviderConfig?> FindByProvider(LlmProvider provider, CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<LlmProviderConfig>().SingleOrDefaultAsync(c => c.Provider == provider, ct);
    }

    public async Task<IReadOnlyList<LlmProviderConfig>> FindAll(CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<LlmProviderConfig>().ToListAsync(ct);
    }
}
