using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;

namespace Nakama.Chatbots.Infrastructure;

public sealed class DiskChatbotDocumentStorage(string basePath) : IChatbotDocumentStorage
{
    private const long MaxBytes = 20 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".md", ".markdown", ".txt",
    };

    public async Task<string> Save(Guid documentId, string originalFileName, Stream content, long length, CancellationToken ct)
    {
        var extension = Path.GetExtension(originalFileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw ChatbotErrors.DocumentUnsupportedType;
        }

        if (length > MaxBytes)
        {
            throw ChatbotErrors.DocumentTooLarge(MaxBytes);
        }

        Directory.CreateDirectory(basePath);
        var fileName = $"{documentId}{extension}";
        await using (var stream = File.Create(Path.Combine(basePath, fileName)))
        {
            await content.CopyToAsync(stream, ct);
        }

        return fileName;
    }

    public void Delete(string fileName)
    {
        var path = Path.Combine(basePath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
