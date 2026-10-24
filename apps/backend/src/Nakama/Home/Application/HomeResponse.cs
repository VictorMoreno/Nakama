namespace Nakama.Home.Application;

public sealed record HomeResponse(
    int QuestionsThisWeek,
    int ActiveDaysLast30,
    IReadOnlyList<HomeFormationRow> Formations,
    IReadOnlyList<HomePendingExamRow> PendingExams);

public sealed record HomeFormationRow(
    Guid FormationId,
    string Title,
    int ProgressPct,
    int CompletedModules,
    int ModulesCount,
    string? NextModuleTitle,
    bool Completed);

public sealed record HomePendingExamRow(
    Guid FormationId,
    string FormationTitle,
    Guid ModuleId,
    string ModuleTitle,
    int LastScore,
    int PassingScore);
