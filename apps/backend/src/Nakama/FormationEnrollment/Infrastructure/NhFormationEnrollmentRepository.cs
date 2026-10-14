using Nakama.FormationEnrollment.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.FormationEnrollment.Infrastructure;

using FormationEnrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

public sealed class NhFormationEnrollmentRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<FormationEnrollment, FormationEnrollmentId>(writeSessions), IFormationEnrollmentRepository;
