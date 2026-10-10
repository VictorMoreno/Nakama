namespace Nakama.Shared.Infrastructure.Llm;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public required int MaxTokens { get; init; }
}
