using Nakama.ChatbotSummaries.Domain;

namespace Nakama.ChatbotSummaries.Application;

public sealed record ChatbotSummaryResponse(
    Guid Id,
    string Label,
    string Description,
    string Status,
    int DocsCount,
    int TeamCount,
    bool General,
    bool Archived,
    string? IconUrl)
{
    public static explicit operator ChatbotSummaryResponse(ChatbotSummary entity) => new(
        entity.Id,
        entity.Label,
        entity.Description,
        entity.Status,
        entity.DocsCount,
        entity.TeamCount,
        entity.General,
        entity.Archived,
        entity.IconUrl is null ? null : $"/api/chatbots/icons/{entity.IconUrl}");
}
