using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.FormationsList.Infrastructure;

public sealed class NhFormationSummaryRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<FormationSummary, Guid>(readSessions), IFormationSummaryRepository;
