using Nakama.Formations.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.Formations.Infrastructure;

public sealed class NhFormationsRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<Formation, FormationId>(unitOfWork), IFormationsRepository
{
    // FindById, Save inherited directly from the base, no redeclaration needed
}
