using Nakama.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotMother
{
    public static Chatbot Create(
        ChatbotId id,
        string label,
        string description,
        ChatbotStatus status,
        ChatbotSettings settings,
        DateTime lastUsed,
        IReadOnlyList<TeamMember> team)
    {
        Chatbot chatbot = new(id, label, description, status, settings, lastUsed);

        foreach (TeamMember member in team)
        {
            chatbot.AddTeamMember(member);
        }

        return chatbot;
    }

    public static Chatbot Random() => With();

    public static ChatbotId RandomId() => ChatbotId.From(Guid.NewGuid());

    public static Chatbot With(
        ChatbotId? id = null,
        string? label = null,
        string? description = null,
        ChatbotStatus? status = null,
        ChatbotSettings? settings = null,
        DateTime? lastUsed = null,
        IReadOnlyList<TeamMember>? team = null) =>
        Create(
            id ?? RandomId(),
            label ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? ChatbotStatus.Active,
            settings ?? ChatbotSettings.Default,
            lastUsed ?? DateTime.UtcNow,
            team ?? []);
}
