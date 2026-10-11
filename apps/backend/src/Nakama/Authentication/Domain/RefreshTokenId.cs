using Nakama.Shared.Domain;

namespace Nakama.Authentication.Domain;

public readonly record struct RefreshTokenId(Guid Value) : IGuidValueObject
{
    public static RefreshTokenId From(Guid value) => new(value);

    public static RefreshTokenId New() => new(Guid.NewGuid());

    public override string ToString() => this.Value.ToString();
}
