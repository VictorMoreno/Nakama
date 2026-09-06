namespace Nakama.Chatbots.Domain;

/// <summary>Mirrors the frontend's <c>LlmProvider</c> ("anthropic" | "openai" | "local").</summary>
public enum LlmProvider
{
    Anthropic,
    OpenAi,
    Local,
}
