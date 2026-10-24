namespace Nakama.Formations.Domain;

public interface IFormationDocumentStorage
{
    Task<string> Save(Guid documentId, string originalFileName, Stream content, long length, CancellationToken ct);

    void Delete(string fileName);
}
