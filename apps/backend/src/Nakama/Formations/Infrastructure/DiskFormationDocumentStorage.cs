using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Infrastructure.Documents;

namespace Nakama.Formations.Infrastructure;

public sealed class DiskFormationDocumentStorage(string basePath, DocumentsOptions options) : IFormationDocumentStorage
{
    private readonly long _maxBytes = options.MaxSizeMb * 1024L * 1024L;

    private readonly HashSet<string> _allowedExtensions = new(options.AllowedExtensions, StringComparer.OrdinalIgnoreCase);

    public async Task<string> Save(Guid documentId, string originalFileName, Stream content, long length, CancellationToken ct)
    {
        string extension = Path.GetExtension(originalFileName);
        if (!this._allowedExtensions.Contains(extension))
        {
            throw FormationErrors.DocumentUnsupportedType;
        }

        if (length > this._maxBytes)
        {
            throw FormationErrors.DocumentTooLarge(this._maxBytes);
        }

        Directory.CreateDirectory(basePath);
        string fileName = $"{documentId}{extension}";
        await using (FileStream stream = File.Create(Path.Combine(basePath, fileName)))
        {
            await content.CopyToAsync(stream, ct);
        }

        return fileName;
    }

    public void Delete(string fileName)
    {
        string path = Path.Combine(basePath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
