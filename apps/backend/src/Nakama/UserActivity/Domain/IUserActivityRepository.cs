namespace Nakama.UserActivity.Domain;

public interface IUserActivityRepository
{
    Task Save(UserActivityEntry entry, CancellationToken ct);
}
