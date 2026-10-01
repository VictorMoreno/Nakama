using Nakama.Shared.Domain;

namespace Nakama.ConversationAudit.Domain;

// Read-model style entity (IEntity, not AggregateRoot) populated exclusively by handlers
// reacting to Conversation's domain events - same shape as ChatPage/Analytics. Deliberately
// never reacts to ConversationDeleted: that's the whole point of this bounded context existing
// separately from the live Conversation/ChatPage.History the user can delete.
public partial class ConversationAuditLog : IEntity
{
    protected ConversationAuditLog() { } // NHibernate

    public virtual Guid Id { get; protected set; }
    public virtual Guid UserId { get; protected set; }
    public virtual Guid BotKey { get; protected set; }
    public virtual string Title { get; protected set; } = string.Empty;
    public virtual DateTime StartedAt { get; protected set; }
    public virtual List<AuditMessageRow> Messages { get; protected set; } = [];
}
