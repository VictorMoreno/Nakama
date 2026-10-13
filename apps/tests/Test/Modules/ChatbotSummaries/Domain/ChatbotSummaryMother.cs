using Nakama.ChatbotSummaries.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ChatbotSummaries.Domain;

public static class ChatbotSummaryMother
{
    public static ChatbotSummary Create(
        Guid id,
        string label,
        string description,
        string status,
        int docsCount,
        int teamCount,
        bool archived,
        string? iconUrl) =>
        new(id, label, description, status, docsCount, teamCount, archived, iconUrl);

    public static ChatbotSummary Random() => With();

    public static ChatbotSummary With(
        Guid? id = null,
        string? label = null,
        string? description = null,
        string? status = null,
        int? docsCount = null,
        int? teamCount = null,
        bool? archived = null,
        string? iconUrl = null) =>
        Create(
            id ?? Guid.NewGuid(),
            label ?? WordMother.Random(),
            description ?? WordMother.Random(),
            status ?? "Active",
            docsCount ?? MotherCreator.Random.Random.Number(0, 10),
            teamCount ?? MotherCreator.Random.Random.Number(1, 10),
            archived ?? false,
            iconUrl);
}
