namespace Nakama.Formations.Domain;

public interface IFormationsRepository
{
    Task<Formation?> FindById(FormationId id, CancellationToken ct);

    Task Save(Formation formation, CancellationToken ct);
}
