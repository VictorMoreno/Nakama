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
    int CompletedModulesCount,
    bool IsEnrolled);
