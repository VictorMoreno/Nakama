using Nakama.Shared.Domain;

namespace Nakama.ConversationHistory.Domain;

public partial class ConversationHistory : IEntity
{
    protected ConversationHistory() { }

    public Guid Id { get; protected set; }
    public List<ConversationSummaryRow> Entries { get; protected set; } = [];
}
