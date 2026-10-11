using Nakama.LlmProviders.Domain.Events;
using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Domain;

public partial class LlmProviderConfig
{
    private LlmProviderConfig(LlmProviderConfigId id, LlmProvider provider, string apiKey, bool enabled,
        IReadOnlyList<LlmModelOption> models, DateTime updatedAt)
    {
        this.Id = id;
        this.Provider = provider;
        this.ApiKey = apiKey;
        this.Enabled = enabled;
        this._models.AddRange(models);
        this.UpdatedAt = updatedAt;
    }

    public static LlmProviderConfig Register(LlmProvider provider, string apiKey, bool enabled, IReadOnlyList<LlmModelOption> models,
        ISystemClock systemClock)
    {
        LlmProviderConfig config = new(LlmProviderConfigId.From(Guid.NewGuid()), provider, apiKey, enabled, models, systemClock.UtcNow);
        config.Record(new LlmProviderConfigured(config.Id, provider, enabled, models));
        return config;
    }

    public void Update(string apiKey, bool enabled, IReadOnlyList<LlmModelOption> models, ISystemClock systemClock)
    {
        this.ApiKey = apiKey;
        this.Enabled = enabled;
        this._models.Clear();
        this._models.AddRange(models);
        this.UpdatedAt = systemClock.UtcNow;

        this.Record(new LlmProviderConfigured(this.Id, this.Provider, enabled, models));
    }
}
