using Nakama.Shared.Domain;

namespace Nakama.ConversationAudit.Domain;

public partial class ConversationAuditLog : IEntity
{
    protected ConversationAuditLog() { } // NHibernate

    public Guid Id { get; protected set; }
    public Guid UserId { get; protected set; }
    public Guid BotKey { get; protected set; }
    public string Title { get; protected set; } = string.Empty;
    public DateTime StartedAt { get; protected set; }
    public List<AuditMessageRow> Messages { get; protected set; } = [];
}
