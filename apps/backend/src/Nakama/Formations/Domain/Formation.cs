using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain;

public partial class Formation : AggregateRoot<FormationId>
{
    protected Formation()
    {
    }

    public virtual string Title { get; private set; } = string.Empty;

    public virtual string Audience { get; private set; } = string.Empty;

    public virtual string Description { get; private set; } = string.Empty;

    public virtual FormationStatus Status { get; private set; }

    public virtual Guid? LinkedChatbotKey { get; private set; }

    public virtual bool Archived { get; private set; }

    public virtual string? IconUrl { get; private set; }

    public virtual LearnerProgress MyProgress { get; private set; } = LearnerProgress.Empty;

    private readonly List<FormationModule> _modules = [];
    public virtual IReadOnlyCollection<FormationModule> Modules => _modules;

    private readonly List<FormationMember> _team = [];
    public virtual IReadOnlyCollection<FormationMember> Team => _team;
}
