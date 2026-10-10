namespace Nakama.Chatbots.Domain;

public interface IConversationAttachmentStorage
{
    Task<string> Save(Guid attachmentId, string originalFileName, byte[] content, CancellationToken ct);

    Task<byte[]?> Read(string fileName, CancellationToken ct);

    void Delete(IEnumerable<string> fileNames);
}
