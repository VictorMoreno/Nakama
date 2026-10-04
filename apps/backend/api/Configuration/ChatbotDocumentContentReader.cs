using Microsoft.Extensions.Options;
using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Storage;

namespace Api.Configuration;

public sealed class ChatbotDocumentContentReader(IOptions<StorageOptions> storage, IWebHostEnvironment env) : IChatbotDocumentContentReader
{
    private static readonly HashSet<string> ReadableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".md", ".markdown", ".txt",
    };

    public async Task<string?> ReadText(string fileName, CancellationToken ct)
    {
        var extension = Path.GetExtension(fileName);
        if (!ReadableExtensions.Contains(extension))
        {
            return null;
        }

        var basePath = FileStorage.ResolvePath(storage.Value.ChatbotDocumentsPath, env);
        var path = Path.Combine(basePath, fileName);
        return File.Exists(path) ? await File.ReadAllTextAsync(path, ct) : null;
    }
}
