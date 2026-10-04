using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;

namespace Nakama.Formations.Infrastructure;

public sealed class DiskFormationIconStorage(string basePath) : IFormationIconStorage
{
    private const long MaxBytes = 2 * 1024 * 1024;

    private static readonly Dictionary<string, string> ExtensionsByContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/jpg"] = ".jpg",
    };

    public async Task<string> Save(Guid id, string contentType, Stream content, long length, CancellationToken ct)
    {
        if (!ExtensionsByContentType.TryGetValue(contentType, out var extension))
        {
            throw FormationErrors.IconUnsupportedType;
        }

        if (length > MaxBytes)
        {
            throw FormationErrors.IconTooLarge(MaxBytes);
        }

        this.DeleteExisting(id);

        Directory.CreateDirectory(basePath);
        var fileName = $"{id}{extension}";
        await using (var stream = File.Create(Path.Combine(basePath, fileName)))
        {
            await content.CopyToAsync(stream, ct);
        }

        return fileName;
    }

    public void DeleteExisting(Guid id)
    {
        foreach (var existing in Directory.EnumerateFiles(basePath, $"{id}.*"))
        {
            File.Delete(existing);
        }
    }
}
