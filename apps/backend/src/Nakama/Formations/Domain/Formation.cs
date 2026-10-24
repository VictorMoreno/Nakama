using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain;

public partial class Formation : AggregateRoot<FormationId>
{
    protected Formation()
    {
    }

    public string Title { get; private set; } = string.Empty;

    public string Audience { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public FormationStatus Status { get; private set; }

    public bool Archived { get; private set; }

    public string? IconUrl { get; private set; }

    private readonly List<FormationModule> _modules = [];
    public IReadOnlyCollection<FormationModule> Modules => this._modules;

    private readonly List<FormationDocument> _documents = [];
    public IReadOnlyCollection<FormationDocument> Documents => this._documents;
}
