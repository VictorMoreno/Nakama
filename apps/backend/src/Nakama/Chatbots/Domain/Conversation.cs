using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public partial class Conversation : AggregateRoot<ConversationId>
{
    protected Conversation()
    {
    }

    public ChatbotId BotKey { get; private set; }

    public Guid UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public DateTime StartedAt { get; private set; }

    private readonly List<ConversationExchange> _exchanges = [];
    public IReadOnlyCollection<ConversationExchange> Exchanges => this._exchanges;
}
