using Nakama.Shared.Domain;

namespace Nakama.FormationsList.Domain;

/// <summary>Read entity behind the <c>/formacion</c> listing route. Fed by <c>Formation</c> events.</summary>
public partial class FormationSummary : IEntity
{
    protected FormationSummary() { } // NHibernate

    public virtual string Key { get; protected set; } = string.Empty;
    public virtual string Title { get; protected set; } = string.Empty;
    public virtual string Audience { get; protected set; } = string.Empty;
    public virtual string Description { get; protected set; } = string.Empty;
    public virtual string Status { get; protected set; } = string.Empty;
    public virtual bool Archived { get; protected set; }
    public virtual string? IconUrl { get; protected set; }

    /// <summary>Computed from <see cref="ModulesCount"/>/<see cref="CompletedModulesCount"/> whenever either changes.</summary>
    public virtual int ProgressPct { get; protected set; }

    public virtual int ModulesCount { get; protected set; }

    public virtual int CompletedModulesCount { get; protected set; }
}
