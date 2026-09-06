using Nakama.Shared.Domain;

namespace Nakama.ChatPage.Domain;

/// <summary>
/// Read entity behind the <c>/chat</c> route - the plan's clearest example of a cross-aggregate
/// read model, fed by both <c>Chatbot</c> (label/settings) and <c>Conversation</c> (history)
/// events without either aggregate referencing the other's Domain type.
/// </summary>
public partial class ChatPage : IEntity
{
    protected ChatPage() { } // NHibernate

    public virtual string BotKey { get; protected set; } = string.Empty;
    public virtual string Label { get; protected set; } = string.Empty;
    public virtual string Context { get; protected set; } = string.Empty;
    public virtual bool Socratic { get; protected set; }
    public virtual bool ExplicaMas { get; protected set; }
    public virtual string Provider { get; protected set; } = string.Empty;

    public virtual ChatExampleRow? Opening { get; protected set; }
    public virtual List<ChatExampleRow> Chips { get; protected set; } = [];
    public virtual List<ConversationSummaryRow> History { get; protected set; } = [];
}
