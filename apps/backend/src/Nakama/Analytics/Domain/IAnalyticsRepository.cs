namespace Nakama.Analytics.Domain;

public interface IAnalyticsRepository
{
    Task<Analytics?> FindSingleton(CancellationToken ct);

    Task Save(Analytics entry, CancellationToken ct);
}
