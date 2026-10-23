using Nakama.Shared.Domain.Specification;

namespace Nakama.UserActivity.Domain;

public interface IUserActivityRepository
{
    Task<IReadOnlyList<UserActivityEntry>> Search(ISpecification<UserActivityEntry> specification, CancellationToken ct);

    Task<int> Count(ISpecification<UserActivityEntry> specification, CancellationToken ct);

    Task Save(UserActivityEntry entry, CancellationToken ct);
}
