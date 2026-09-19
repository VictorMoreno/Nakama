namespace Nakama.FormationDetail.Domain;

public interface IFormationDetailRepository
{
    Task<FormationDetail?> FindById(Guid id, CancellationToken ct);

    Task Save(FormationDetail entry, CancellationToken ct);

    Task Update(FormationDetail entry, CancellationToken ct);
}
