namespace Nakama.Chatbots.Domain;

public sealed record ChatbotSettings(bool Socratic, bool ExplicaMas, LlmProvider Provider)
{
    public static ChatbotSettings Default { get; } = new(Socratic: false, ExplicaMas: false, Provider: LlmProvider.Anthropic);
}
