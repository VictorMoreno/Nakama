using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public partial class Chatbot : AggregateRoot<ChatbotId>
{
    protected Chatbot()
    {
    }

    public virtual string Label { get; private set; } = string.Empty;

    public virtual string Description { get; private set; } = string.Empty;

    public virtual ChatbotStatus Status { get; private set; }

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
