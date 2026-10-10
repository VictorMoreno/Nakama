using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Infrastructure.Chat;

namespace Nakama.Chatbots.Infrastructure;

public sealed class DiskConversationAttachmentStorage(string basePath, ChatOptions options) : IConversationAttachmentStorage
{
    private readonly long _maxBytes = options.MaxAttachmentSizeMb * 1024L * 1024L;

    private readonly HashSet<string> _allowedExtensions = new(options.AllowedAttachmentExtensions, StringComparer.OrdinalIgnoreCase);

    public async Task<string> Save(Guid attachmentId, string originalFileName, byte[] content, CancellationToken ct)
    {
        string extension = Path.GetExtension(originalFileName);
        if (!this._allowedExtensions.Contains(extension))
        {
            throw ChatbotErrors.DocumentUnsupportedType;
        }

        if (content.LongLength > this._maxBytes)
        {
            throw ChatbotErrors.DocumentTooLarge(this._maxBytes);
        }

        Directory.CreateDirectory(basePath);
        string fileName = $"{attachmentId}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(basePath, fileName), content, ct);
        return fileName;
    }

    public async Task<byte[]?> Read(string fileName, CancellationToken ct)
    {
        string path = Path.Combine(basePath, fileName);
        return File.Exists(path) ? await File.ReadAllBytesAsync(path, ct) : null;
    }

    public void Delete(IEnumerable<string> fileNames)
    {
        foreach (string fileName in fileNames)
        {
            string path = Path.Combine(basePath, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
