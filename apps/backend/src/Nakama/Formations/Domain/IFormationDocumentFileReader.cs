namespace Nakama.Formations.Domain;

public interface IFormationDocumentFileReader
{
    Task<byte[]?> Read(string fileName, CancellationToken ct);
}
