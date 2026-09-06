namespace Nakama.FormationDetail.Domain;

public interface IFormationDetailRepository
{
    Task<FormationDetail?> FindByKey(string key, CancellationToken ct);

    Task Save(FormationDetail entry, CancellationToken ct);
}
