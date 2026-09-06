using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

/// <summary>
/// Structural half of the aggregate (properties only) - split from behavior/constructors in
/// <c>Chatbot.domain.cs</c> per agents.md §3, mirroring the split a future entity generator
/// would produce. Protects two invariants: an owner can't be removed from the team, and status
/// transitions/archival are explicit, event-raising operations rather than free property sets.
/// </summary>
public partial class Chatbot : AggregateRoot<ChatbotKey>
{
    /// <summary>Required by NHibernate for proxies/hydration - never used by hand.</summary>
    protected Chatbot()
    {
    }

    public virtual ChatbotKey Key => this.Id;

    public virtual string Label { get; private set; } = string.Empty;

    public virtual string Description { get; private set; } = string.Empty;

    public virtual ChatbotStatus Status { get; private set; }

    /// <summary>HTML context shown in the chat panel - distinct from <see cref="Description"/>, which is the short listing blurb.</summary>
    public virtual string Context { get; private set; } = string.Empty;

    public virtual bool General { get; private set; }

    public virtual bool Archived { get; private set; }

    public virtual string? IconUrl { get; private set; }

    public virtual ChatbotSettings Settings { get; private set; } = ChatbotSettings.Default;

    public virtual DateTime LastUsed { get; private set; }

    private readonly List<ChatbotDocument> _documents = [];
    public virtual IReadOnlyCollection<ChatbotDocument> Documents => _documents;

    private readonly List<TeamMember> _team = [];
    public virtual IReadOnlyCollection<TeamMember> Team => _team;
}
