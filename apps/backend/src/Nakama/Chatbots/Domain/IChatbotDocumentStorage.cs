namespace Nakama.Chatbots.Domain;

public interface IChatbotDocumentStorage
{
    Task<string> Save(Guid documentId, string originalFileName, Stream content, long length, CancellationToken ct);

    void Delete(string fileName);
}
