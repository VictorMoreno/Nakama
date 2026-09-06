using Nakama.FormationsList.Domain;

namespace Nakama.FormationsList.Application;

public sealed record FormationSummaryResponse(
    string Key,
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
        entity.Key,
        entity.Title,
        entity.Audience,
        entity.Description,
        entity.Status,
        entity.Archived,
        entity.IconUrl,
        entity.ProgressPct,
        entity.ModulesCount,
        entity.CompletedModulesCount);
}
