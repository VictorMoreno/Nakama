using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain;

/// <summary>
/// Structural half of the aggregate - split from behavior/constructors in
/// <c>Formation.domain.cs</c> per agents.md §3, mirroring <c>Chatbot</c>/<c>Chatbot.domain.cs</c>.
/// <see cref="LinkedChatbotKey"/> is a plain string, never <c>Chatbots.Domain.ChatbotKey</c> -
/// a bounded context must not reference another context's Domain, per agents.md §1.
/// </summary>
public partial class Formation : AggregateRoot<FormationKey>
{
    /// <summary>Required by NHibernate for proxies/hydration - never used by hand.</summary>
    protected Formation()
    {
    }

    public virtual FormationKey Key => this.Id;

    public virtual string Title { get; private set; } = string.Empty;

    public virtual string Audience { get; private set; } = string.Empty;

    public virtual string Description { get; private set; } = string.Empty;

    public virtual FormationStatus Status { get; private set; }

    public virtual string? LinkedChatbotKey { get; private set; }

    public virtual bool Archived { get; private set; }

    public virtual string? IconUrl { get; private set; }

    public virtual LearnerProgress MyProgress { get; private set; } = LearnerProgress.Empty;

    private readonly List<FormationModule> _modules = [];
    public virtual IReadOnlyCollection<FormationModule> Modules => _modules;

    private readonly List<FormationMember> _team = [];
    public virtual IReadOnlyCollection<FormationMember> Team => _team;
}
