using Nakama.Shared.Domain;

namespace Nakama.ConversationHistory.Domain;

public partial class ConversationHistory : IEntity
{
    protected ConversationHistory() { } // NHibernate

    public Guid Id { get; protected set; } // = botKey
    public List<ConversationSummaryRow> Entries { get; protected set; } = [];
}
