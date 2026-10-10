using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Infrastructure.Icons;

namespace Nakama.Formations.Infrastructure;

public sealed class DiskFormationIconStorage(string basePath, IconsOptions options) : IFormationIconStorage
{
    private readonly long _maxBytes = options.MaxSizeMb * 1024L * 1024L;

    private readonly Dictionary<string, string> _extensionsByContentType = new(options.ContentTypeExtensions, StringComparer.OrdinalIgnoreCase);

    public async Task<string> Save(Guid id, string contentType, Stream content, long length, CancellationToken ct)
    {
        if (!this._extensionsByContentType.TryGetValue(contentType, out string? extension))
        {
            throw FormationErrors.IconUnsupportedType;
        }

        if (length > this._maxBytes)
        {
            throw FormationErrors.IconTooLarge(this._maxBytes);
        }

        this.DeleteExisting(id);

        Directory.CreateDirectory(basePath);
        string fileName = $"{id}{extension}";
        await using (FileStream stream = File.Create(Path.Combine(basePath, fileName)))
        {
            await content.CopyToAsync(stream, ct);
        }

        return fileName;
    }

    public void DeleteExisting(Guid id)
    {
        foreach (string existing in Directory.EnumerateFiles(basePath, $"{id}.*"))
        {
            File.Delete(existing);
        }
    }
}
