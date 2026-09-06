namespace Nakama.Formations.Domain;

/// <summary>
/// A module within a formation curriculum. Which fields are relevant depends on
/// <see cref="Type"/>: video uses <see cref="ResourceUrl"/>+<see cref="DurationMin"/>, document
/// and exercise use <see cref="ResourceUrl"/>, exam uses <see cref="Quiz"/>+<see cref="PassingScore"/>.
/// Immutable - <see cref="Formation.ToggleModuleGating"/> replaces the list entry with a
/// <c>with</c>-copy rather than mutating a field on it.
/// </summary>
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
