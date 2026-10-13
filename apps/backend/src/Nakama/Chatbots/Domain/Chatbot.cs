using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public partial class Chatbot : AggregateRoot<ChatbotId>
{
    protected Chatbot()
    {
    }

    public string Label { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public ChatbotStatus Status { get; private set; }

    public bool Archived { get; private set; }

    public string? IconUrl { get; private set; }

    public ChatbotSettings Settings { get; private set; } = ChatbotSettings.Default;

    private readonly List<ChatbotDocument> _documents = [];
    public IReadOnlyCollection<ChatbotDocument> Documents => this._documents;

    private readonly List<TeamMember> _team = [];
    public IReadOnlyCollection<TeamMember> Team => this._team;
}
