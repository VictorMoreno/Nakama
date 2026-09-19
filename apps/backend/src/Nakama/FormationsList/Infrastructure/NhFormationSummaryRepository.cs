using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.FormationsList.Infrastructure;

public sealed class NhFormationSummaryRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<FormationSummary, Guid>(readSessions), IFormationSummaryRepository
{
    public Task<IReadOnlyCollection<FormationSummary>> FindAll(CancellationToken ct) => this.FindAllEntities(ct);

    // FindById, Save inherited directly from the base, no redeclaration needed
}
