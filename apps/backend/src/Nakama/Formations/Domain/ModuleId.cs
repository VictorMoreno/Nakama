using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain;

public readonly record struct ModuleId(string Value) : IStringValueObject
{
    public static ModuleId From(string value) => new(value);

    public override string ToString() => this.Value;
}
