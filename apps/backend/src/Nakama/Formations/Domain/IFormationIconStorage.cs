namespace Nakama.Formations.Domain;

public interface IFormationIconStorage
{
    Task<string> Save(Guid id, string contentType, Stream content, long length, CancellationToken ct);

    void DeleteExisting(Guid id);
}
