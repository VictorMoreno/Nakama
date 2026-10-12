using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Domain;

public partial class LlmProviderConfig : AggregateRoot<LlmProviderConfigId>
{
    protected LlmProviderConfig()
    {
    }

    public LlmProvider Provider { get; private set; }

    public string ApiKey { get; private set; } = string.Empty;

    public bool Enabled { get; private set; }

    private readonly List<LlmModelOption> _models = [];
    public IReadOnlyList<LlmModelOption> Models => this._models;

    public DateTime UpdatedAt { get; private set; }
}
