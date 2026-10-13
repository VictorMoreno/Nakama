using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Domain;

public partial class LlmProviderConfig
{
    public LlmProviderConfig(LlmProviderConfigId id, LlmProvider provider, string apiKey, bool enabled,
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
        => new(LlmProviderConfigId.From(Guid.NewGuid()), provider, apiKey, enabled, models, systemClock.UtcNow);

    public void Update(string apiKey, bool enabled, IReadOnlyList<LlmModelOption> models, ISystemClock systemClock)
    {
        this.ApiKey = apiKey;
        this.Enabled = enabled;
        this._models.Clear();
        this._models.AddRange(models);
        this.UpdatedAt = systemClock.UtcNow;
    }
}
