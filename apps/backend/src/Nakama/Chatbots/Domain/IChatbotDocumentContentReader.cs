namespace Nakama.Chatbots.Domain;

public interface IChatbotDocumentContentReader
{
    Task<string?> ReadText(string fileName, CancellationToken ct);
}
