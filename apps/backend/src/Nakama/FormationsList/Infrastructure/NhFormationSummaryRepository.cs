using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.FormationsList.Infrastructure;

public sealed class NhFormationSummaryRepository(IReadSessionFactory readSessions)
    : NhRepositoryBase<FormationSummary, string>(readSessions), IFormationSummaryRepository
{
    public Task<FormationSummary?> FindByKey(string key, CancellationToken ct) => this.FindById(key, ct);

    public Task<IReadOnlyCollection<FormationSummary>> FindAll(CancellationToken ct) => this.FindAllEntities(ct);
}
