using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public partial class Conversation : AggregateRoot<ConversationId>
{
    protected Conversation()
    {
    }

    public virtual ChatbotId BotKey { get; private set; }

    public virtual string Title { get; private set; } = string.Empty;

    public virtual DateTime StartedAt { get; private set; }

    private readonly List<ConversationExchange> _exchanges = [];
    public virtual IReadOnlyCollection<ConversationExchange> Exchanges => _exchanges;
}
