using Nakama.Formations.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.Formations.Infrastructure;

public sealed class NhFormationsRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<Formation, FormationKey>(writeSessions), IFormationsRepository
{
    public Task<Formation?> FindByKey(FormationKey key, CancellationToken ct) => this.FindById(key, ct);
}
