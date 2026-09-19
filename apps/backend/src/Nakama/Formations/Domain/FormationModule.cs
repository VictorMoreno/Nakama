namespace Nakama.Formations.Domain;

public sealed record FormationModule(
    ModuleId Id,
    string Title,
    ModuleType Type,
    string Summary,
    bool Gated,
    bool AiDraft,
    string? ResourceUrl = null,
    int? DurationMin = null,
    IReadOnlyList<QuizQuestion>? Quiz = null,
    int? PassingScore = null);
