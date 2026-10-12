using Nakama.Shared.Domain;

namespace Nakama.ChatPage.Domain;

public partial class ChatPage : IEntity
{
    protected ChatPage() { } // NHibernate

    public Guid Id { get; protected set; }
    public string Label { get; protected set; } = string.Empty;
    public string Context { get; protected set; } = string.Empty;
    public bool Socratic { get; protected set; }
    public bool ExplicaMas { get; protected set; }
    public string Provider { get; protected set; } = string.Empty;

    public ChatExampleRow? Opening { get; protected set; }
}
