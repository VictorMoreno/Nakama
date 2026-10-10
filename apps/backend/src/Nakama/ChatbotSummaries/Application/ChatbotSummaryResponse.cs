using Nakama.ChatbotSummaries.Domain;

namespace Nakama.ChatbotSummaries.Application;

public sealed record ChatbotSummaryResponse(
    Guid Id,
    string Label,
    string Description,
    string Status,
    int DocsCount,
    int TeamCount,
    bool Archived,
    string? IconUrl)
{
    public static ChatbotSummaryResponse From(ChatbotSummary entity, string iconsRoute) => new(
        entity.Id,
        entity.Label,
        entity.Description,
        entity.Status,
        entity.DocsCount,
        entity.TeamCount,
        entity.Archived,
        entity.IconUrl is null ? null : $"{iconsRoute}/{entity.IconUrl}");
}
