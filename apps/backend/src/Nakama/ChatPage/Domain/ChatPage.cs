using Nakama.Shared.Domain;

namespace Nakama.ChatPage.Domain;

public partial class ChatPage : IEntity
{
    protected ChatPage() { } // NHibernate

    public virtual Guid Id { get; protected set; }
    public virtual string Label { get; protected set; } = string.Empty;
    public virtual string Context { get; protected set; } = string.Empty;
    public virtual bool Socratic { get; protected set; }
    public virtual bool ExplicaMas { get; protected set; }
    public virtual string Provider { get; protected set; } = string.Empty;

    public virtual ChatExampleRow? Opening { get; protected set; }
    public virtual List<ChatExampleRow> Chips { get; protected set; } = [];
    public virtual List<ConversationSummaryRow> History { get; protected set; } = [];
}
