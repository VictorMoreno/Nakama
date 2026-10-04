using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public partial class Conversation : AggregateRoot<ConversationId>
{
    protected Conversation()
    {
    }

    public virtual ChatbotId BotKey { get; private set; }

    // Cross-context reference kept as a plain Guid, not Users.Domain.UserId - same precedent
    // as Formation.LinkedChatbotKey, to avoid a domain-level dependency on another bounded context.
    public virtual Guid UserId { get; private set; }

    public virtual string Title { get; private set; } = string.Empty;

    public virtual DateTime StartedAt { get; private set; }

    private readonly List<ConversationExchange> _exchanges = [];
    public virtual IReadOnlyCollection<ConversationExchange> Exchanges => this._exchanges;
}
