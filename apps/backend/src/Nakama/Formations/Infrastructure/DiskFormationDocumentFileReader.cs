using Nakama.Formations.Domain;

namespace Nakama.Formations.Infrastructure;

public sealed class DiskFormationDocumentFileReader(string basePath) : IFormationDocumentFileReader
{
    public async Task<byte[]?> Read(string fileName, CancellationToken ct)
    {
        string path = Path.Combine(basePath, fileName);
        return File.Exists(path) ? await File.ReadAllBytesAsync(path, ct) : null;
    }
}
