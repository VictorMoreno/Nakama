using Nakama.FormationEnrollment.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using NHibernate;
using NHibernate.Linq;

namespace Nakama.FormationEnrollment.Infrastructure;

using FormationEnrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

public sealed class NhFormationEnrollmentRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<FormationEnrollment, FormationEnrollmentId>(writeSessions), IFormationEnrollmentRepository
{
    public async Task<FormationEnrollment?> FindByFormationAndUser(Guid formationId, Guid userId, CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<FormationEnrollment>()
            .SingleOrDefaultAsync(e => e.FormationId == formationId && e.UserId == userId, ct);
    }

    public async Task<IReadOnlyList<FormationEnrollment>> FindAllForUser(Guid userId, CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<FormationEnrollment>().Where(e => e.UserId == userId).ToListAsync(ct);
    }
}
