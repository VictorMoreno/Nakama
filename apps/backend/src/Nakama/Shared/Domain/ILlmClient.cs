namespace Nakama.Shared.Domain;

public sealed record LlmMessage(string Role, string Content);

public interface ILlmClient
{
    IAsyncEnumerable<string> AskStream(string model, string systemPrompt, IReadOnlyList<LlmMessage> history, CancellationToken ct);
}
