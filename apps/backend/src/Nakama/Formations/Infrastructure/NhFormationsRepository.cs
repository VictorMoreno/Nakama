using Nakama.Formations.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.Formations.Infrastructure;

public sealed class NhFormationsRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<Formation, FormationId>(writeSessions), IFormationsRepository
{
    // FindById, Save inherited directly from the base, no redeclaration needed
}
