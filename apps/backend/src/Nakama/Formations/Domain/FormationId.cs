using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain;

public readonly record struct FormationId(Guid Value) : IGuidValueObject
{
    public static FormationId From(Guid value) => new(value);

    public override string ToString() => this.Value.ToString();
}
