using Nakama.Shared.Domain;

namespace Nakama.ChatbotSummaries.Domain;

public partial class ChatbotSummary : IEntity
{
    protected ChatbotSummary() { } // NHibernate

    public virtual Guid Id { get; protected set; }
    public virtual string Label { get; protected set; } = string.Empty;
    public virtual string Description { get; protected set; } = string.Empty;
    public virtual string Status { get; protected set; } = string.Empty;
    public virtual DateTime LastUsed { get; protected set; }
    public virtual int DocsCount { get; protected set; }
    public virtual int TeamCount { get; protected set; }
    public virtual bool General { get; protected set; }
    public virtual bool Archived { get; protected set; }
    public virtual string? IconUrl { get; protected set; }
}
