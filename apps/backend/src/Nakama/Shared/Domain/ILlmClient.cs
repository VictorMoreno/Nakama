namespace Nakama.Shared.Domain;

public sealed record LlmMessage(string Role, string Content, IReadOnlyList<LlmDocument>? Documents = null);

public enum LlmDocumentKind
{
    Pdf,
    PlainText
}

public sealed record LlmDocument(string Name, LlmDocumentKind Kind, byte[] Content);

public interface ILlmClient
{
    IAsyncEnumerable<string> AskStream(
        string model,
        string systemPrompt,
        IReadOnlyList<LlmMessage> history,
        IReadOnlyList<LlmDocument> documents,
        CancellationToken ct);
}
