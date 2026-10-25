using Nakama.Shared.Domain;

namespace Nakama.FormationsList.Domain;

public partial class FormationSummary : IEntity
{
    protected FormationSummary() { }

    public Guid Id { get; protected set; }
    public string Title { get; protected set; } = string.Empty;
    public string Audience { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;
    public string Status { get; protected set; } = string.Empty;
    public bool Archived { get; protected set; }
    public string? IconUrl { get; protected set; }
    public int ModulesCount { get; protected set; }
}
