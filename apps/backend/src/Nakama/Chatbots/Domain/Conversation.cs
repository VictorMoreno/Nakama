using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

/// <summary>
/// Own aggregate, not a child entity of <see cref="Chatbot"/>: a conversation grows
/// (exchanges) independently of the chatbot's configuration, and requiring the full Chatbot
/// aggregate lock to append an exchange would create needless contention. References
/// <see cref="Chatbot"/> only by <see cref="ChatbotKey"/>, never by object.
/// </summary>
public partial class Conversation : AggregateRoot<ConversationId>
{
    /// <summary>Required by NHibernate for proxies/hydration - never used by hand.</summary>
    protected Conversation()
    {
    }

    public virtual ChatbotKey BotKey { get; private set; }

    public virtual string Title { get; private set; } = string.Empty;

    public virtual DateTime StartedAt { get; private set; }

    private readonly List<ConversationExchange> _exchanges = [];
    public virtual IReadOnlyCollection<ConversationExchange> Exchanges => _exchanges;
}
