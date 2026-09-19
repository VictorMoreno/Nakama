using Nakama.Shared.Domain;

namespace Nakama.FormationDetail.Domain;

public partial class ChatbotLabelRef : IEntity
{
    protected ChatbotLabelRef() { } // NHibernate

    public virtual Guid Id { get; protected set; }
    public virtual string Label { get; protected set; } = string.Empty;
}
