namespace Nakama.Shared.Domain;

public sealed record LlmMessage(string Role, string Content);

public interface ILlmClient
{
    Task<string> Ask(string model, string systemPrompt, IReadOnlyList<LlmMessage> history, CancellationToken ct);
}
