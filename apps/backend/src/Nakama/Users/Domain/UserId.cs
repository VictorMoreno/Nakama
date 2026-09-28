using Nakama.Shared.Domain;

namespace Nakama.Users.Domain;

public readonly record struct UserId(Guid Value) : IGuidValueObject
{
    public static UserId From(Guid value) => new(value);

    public override string ToString() => this.Value.ToString();
}
