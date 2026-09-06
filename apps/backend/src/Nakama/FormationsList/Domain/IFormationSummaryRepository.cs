namespace Nakama.FormationsList.Domain;

public interface IFormationSummaryRepository
{
    Task<FormationSummary?> FindByKey(string key, CancellationToken ct);

    Task<IReadOnlyCollection<FormationSummary>> FindAll(CancellationToken ct);

    Task Save(FormationSummary entry, CancellationToken ct);
}
