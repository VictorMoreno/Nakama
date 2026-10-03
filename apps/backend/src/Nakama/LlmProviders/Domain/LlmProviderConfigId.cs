using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Domain;

public readonly record struct LlmProviderConfigId(Guid Value) : IGuidValueObject
{
    public static LlmProviderConfigId From(Guid value) => new(value);

    public override string ToString() => this.Value.ToString();
}
