using Nakama.FormationsList.Domain;

namespace Nakama.FormationsList.Application;

public sealed record FormationSummaryResponse(
    Guid Id,
    string Title,
    string Audience,
    string Description,
    string Status,
    bool Archived,
    string? IconUrl,
    int ProgressPct,
    int ModulesCount,
    int CompletedModulesCount)
{
    public static explicit operator FormationSummaryResponse(FormationSummary entity) => new(
        entity.Id,
        entity.Title,
        entity.Audience,
        entity.Description,
        entity.Status,
        entity.Archived,
        entity.IconUrl is null ? null : $"/api/formations/icons/{entity.IconUrl}",
        entity.ProgressPct,
        entity.ModulesCount,
        entity.CompletedModulesCount);
}
