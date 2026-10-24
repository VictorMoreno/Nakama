using Nakama.Shared.Domain;

namespace Nakama.FormationDetail.Domain;

public partial class FormationDetail : IEntity
{
    protected FormationDetail() { } // NHibernate

    public Guid Id { get; protected set; }
    public string Title { get; protected set; } = string.Empty;
    public string Audience { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public string Status { get; protected set; } = string.Empty;
    public bool Archived { get; protected set; }
    public string? IconUrl { get; protected set; }

    public List<FormationModuleRow> Modules { get; protected set; } = [];
    public List<FormationMemberRow> Team { get; protected set; } = [];
    public List<FormationDocumentRow> Docs { get; protected set; } = [];
}
