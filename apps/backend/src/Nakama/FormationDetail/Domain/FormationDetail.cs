using Nakama.Shared.Domain;

namespace Nakama.FormationDetail.Domain;

public partial class FormationDetail : IEntity
{
    protected FormationDetail() { } // NHibernate

    public virtual Guid Id { get; protected set; }
    public virtual string Title { get; protected set; } = string.Empty;
    public virtual string Audience { get; protected set; } = string.Empty;
    public virtual string Description { get; protected set; } = string.Empty;
    public virtual string Status { get; protected set; } = string.Empty;
    public virtual bool Archived { get; protected set; }
    public virtual string? IconUrl { get; protected set; }
    public virtual Guid? LinkedChatbotKey { get; protected set; }
    public virtual string? LinkedChatbotLabel { get; protected set; }

    public virtual List<FormationModuleRow> Modules { get; protected set; } = [];
    public virtual List<FormationMemberRow> Team { get; protected set; } = [];
}
