using Nakama.Shared.Domain;

namespace Nakama.FormationDetail.Domain;

public partial class ChatbotLabelRef : IEntity
{
    protected ChatbotLabelRef() { } // NHibernate

    public Guid Id { get; protected set; }
    public string Label { get; protected set; } = string.Empty;
}
