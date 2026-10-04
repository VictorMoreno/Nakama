using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;

namespace Nakama.Chatbots.Infrastructure;

public sealed class DiskChatbotIconStorage(string basePath) : IChatbotIconStorage
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
            throw ChatbotErrors.IconUnsupportedType;
        }

        if (length > MaxBytes)
        {
            throw ChatbotErrors.IconTooLarge(MaxBytes);
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
