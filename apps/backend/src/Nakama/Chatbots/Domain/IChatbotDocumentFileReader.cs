namespace Nakama.Chatbots.Domain;

public interface IChatbotDocumentFileReader
{
    Task<byte[]?> Read(string fileName, CancellationToken ct);
}
