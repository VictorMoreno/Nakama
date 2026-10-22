using Nakama.Analytics.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.Analytics.Infrastructure;

using Analytics = Nakama.Analytics.Domain.Analytics;

public sealed class NhAnalyticsRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<Analytics, string>(unitOfWork), IAnalyticsRepository
{
    public Task<Analytics?> Find(CancellationToken ct) => this.FindById(Analytics.SingletonKey, ct);
}
