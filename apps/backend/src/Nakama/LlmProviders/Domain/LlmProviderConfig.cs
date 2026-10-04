using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Domain;

public partial class LlmProviderConfig : AggregateRoot<LlmProviderConfigId>
{
    protected LlmProviderConfig()
    {
    }

    public virtual LlmProvider Provider { get; private set; }

    public virtual string ApiKey { get; private set; } = string.Empty;

    public virtual bool Enabled { get; private set; }

    private readonly List<LlmModelOption> _models = [];
    public virtual IReadOnlyList<LlmModelOption> Models => this._models;

    public virtual DateTime UpdatedAt { get; private set; }
}
