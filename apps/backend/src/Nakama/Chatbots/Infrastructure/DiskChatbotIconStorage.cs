using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Infrastructure.Icons;

namespace Nakama.Chatbots.Infrastructure;

public sealed class DiskChatbotIconStorage(string basePath, IconsOptions options) : IChatbotIconStorage
{
    private readonly long _maxBytes = options.MaxSizeMb * 1024L * 1024L;

    private readonly Dictionary<string, string> _extensionsByContentType = new(options.ContentTypeExtensions, StringComparer.OrdinalIgnoreCase);

    public async Task<string> Save(Guid id, string contentType, Stream content, long length, CancellationToken ct)
    {
        if (!this._extensionsByContentType.TryGetValue(contentType, out string? extension))
        {
            throw ChatbotErrors.IconUnsupportedType;
        }

        if (length > this._maxBytes)
        {
            throw ChatbotErrors.IconTooLarge(this._maxBytes);
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
