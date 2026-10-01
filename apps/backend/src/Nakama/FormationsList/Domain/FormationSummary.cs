using Nakama.Shared.Domain;

namespace Nakama.FormationsList.Domain;

public partial class FormationSummary : IEntity
{
    protected FormationSummary() { } // NHibernate

    public virtual Guid Id { get; protected set; }
    public virtual string Title { get; protected set; } = string.Empty;
    public virtual string Audience { get; protected set; } = string.Empty;
    public virtual string Description { get; protected set; } = string.Empty;
    public virtual string Status { get; protected set; } = string.Empty;
    public virtual bool Archived { get; protected set; }
    public virtual string? IconUrl { get; protected set; }
    public virtual int ModulesCount { get; protected set; }
}
