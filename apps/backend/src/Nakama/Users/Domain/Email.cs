using Nakama.Users.Domain.Errors;

namespace Nakama.Users.Domain;

public readonly record struct Email
{
    public string Value { get; }

    public string Normalized => this.Value.ToUpperInvariant();

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
        {
            throw UserErrors.InvalidEmail(value);
        }

        this.Value = value;
    }
}
