using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain;

public readonly record struct FormationKey(string Value) : IStringValueObject
{
    public static FormationKey From(string value) => new(value);

    public override string ToString() => this.Value;
}
