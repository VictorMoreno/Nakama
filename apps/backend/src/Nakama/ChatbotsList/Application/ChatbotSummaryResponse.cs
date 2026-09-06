using Nakama.ChatbotsList.Domain;

namespace Nakama.ChatbotsList.Application;

public sealed record ChatbotSummaryResponse(
    string Key,
    string Label,
    string Description,
    string Status,
    DateTime LastUsed,
    int DocsCount,
    int TeamCount,
    bool General,
    bool Archived,
    string? IconUrl)
{
    public static explicit operator ChatbotSummaryResponse(ChatbotSummary entity) => new(
        entity.Key,
        entity.Label,
        entity.Description,
        entity.Status,
        entity.LastUsed,
        entity.DocsCount,
        entity.TeamCount,
        entity.General,
        entity.Archived,
        entity.IconUrl);
}
