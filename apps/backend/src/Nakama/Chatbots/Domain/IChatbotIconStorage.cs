namespace Nakama.Chatbots.Domain;

public interface IChatbotIconStorage
{
    Task<string> Save(Guid id, string contentType, Stream content, long length, CancellationToken ct);

    void DeleteExisting(Guid id);
}
