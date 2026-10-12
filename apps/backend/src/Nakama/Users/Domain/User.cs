using Nakama.Shared.Domain;

namespace Nakama.Users.Domain;

public partial class User : AggregateRoot<UserId>
{
    protected User()
    {
    }

    public string Email { get; private set; } = string.Empty;

    public string NormalizedEmail { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string? PasswordHash { get; private set; }

    public DateTime? LastActiveAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsAdmin { get; private set; }
}
