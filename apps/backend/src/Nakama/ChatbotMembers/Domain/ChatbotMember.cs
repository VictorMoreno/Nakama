using Nakama.Shared.Domain;

namespace Nakama.ChatbotMembers.Domain;

public partial class ChatbotMember : IEntity
{
    protected ChatbotMember() { } // NHibernate

    public Guid Id { get; protected set; }
    public Guid ChatbotId { get; protected set; }
    public Guid UserId { get; protected set; }
}
