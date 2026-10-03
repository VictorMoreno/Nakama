using Nakama.Chatbots.Domain;

namespace Api.Configuration;

public sealed class ChatbotDocumentContentReader(IConfiguration configuration, IWebHostEnvironment env) : IChatbotDocumentContentReader
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

        var basePath = DocumentStorage.ResolvePath(configuration, env, "ChatbotDocumentsPath");
        var path = Path.Combine(basePath, fileName);
        return File.Exists(path) ? await File.ReadAllTextAsync(path, ct) : null;
    }
}
