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
        string? iconUrl,
        bool archived,
        IReadOnlyList<TeamMember> team,
        IReadOnlyList<ChatbotDocument> documents) =>
        new(id, label, description, status, settings, iconUrl, archived, team, documents);

    public static Chatbot Random() => With();

    public static Chatbot Archived() => With(archived: true);

    public static ChatbotId RandomId() => ChatbotId.From(Guid.NewGuid());

    public static Chatbot With(
        ChatbotId? id = null,
        string? label = null,
        string? description = null,
        ChatbotStatus? status = null,
        ChatbotSettings? settings = null,
        string? iconUrl = null,
        bool? archived = null,
        IReadOnlyList<TeamMember>? team = null,
        IReadOnlyList<ChatbotDocument>? documents = null) =>
        Create(
            id ?? RandomId(),
            label ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? ChatbotStatus.Active,
            settings ?? ChatbotSettings.Default,
            iconUrl,
            archived ?? false,
            team ?? [],
            documents ?? []);
}
