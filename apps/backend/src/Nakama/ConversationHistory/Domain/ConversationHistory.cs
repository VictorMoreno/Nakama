using Nakama.Shared.Domain;

namespace Nakama.ConversationHistory.Domain;

// Read-model style entity (IEntity, not AggregateRoot), populated exclusively by handlers
// reacting to Conversation's domain events - same shape as ChatPage/Analytics/ConversationAudit.
// Deliberately has no relationship (navigation property) to ChatPage in either direction;
// the two only ever share a BotKey value, never an object reference.
public partial class ConversationHistory : IEntity
{
    protected ConversationHistory() { } // NHibernate

    public virtual Guid Id { get; protected set; } // = botKey
    public virtual List<ConversationSummaryRow> Entries { get; protected set; } = [];
}
