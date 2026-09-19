using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.FormationDetail.Infrastructure;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

public sealed class NhFormationDetailRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<FormationDetail, Guid>(readSessions), IFormationDetailRepository
{
    // FindById, Save inherited directly from the base, no redeclaration needed
}
