namespace Nakama.FormationsList.Domain;

public interface IFormationSummaryRepository
{
    Task<FormationSummary?> FindById(Guid id, CancellationToken ct);

    Task<IReadOnlyCollection<FormationSummary>> FindAll(CancellationToken ct);

    Task Save(FormationSummary entry, CancellationToken ct);

    Task Update(FormationSummary entry, CancellationToken ct);
}
