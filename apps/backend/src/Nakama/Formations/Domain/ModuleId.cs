using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain;

public readonly record struct ModuleId(Guid Value) : IGuidValueObject
{
    public static ModuleId From(Guid value) => new(value);

    public override string ToString() => this.Value.ToString();
}
