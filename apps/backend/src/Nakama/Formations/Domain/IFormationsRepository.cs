namespace Nakama.Formations.Domain;

public interface IFormationsRepository
{
    Task<Formation?> FindByKey(FormationKey key, CancellationToken ct);

    Task Save(Formation formation, CancellationToken ct);
}
