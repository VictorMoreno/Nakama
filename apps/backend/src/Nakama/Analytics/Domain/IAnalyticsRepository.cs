namespace Nakama.Analytics.Domain;

public interface IAnalyticsRepository
{
    Task<Analytics?> Find(CancellationToken ct);

    Task Save(Analytics entry, CancellationToken ct);

    Task Update(Analytics entry, CancellationToken ct);
}
