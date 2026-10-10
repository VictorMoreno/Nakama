using Nakama.Chatbots.Domain;

namespace Nakama.Chatbots.Infrastructure;

public sealed class DiskChatbotDocumentFileReader(string basePath) : IChatbotDocumentFileReader
{
    public async Task<byte[]?> Read(string fileName, CancellationToken ct)
    {
        string path = Path.Combine(basePath, fileName);
        return File.Exists(path) ? await File.ReadAllBytesAsync(path, ct) : null;
    }
}
