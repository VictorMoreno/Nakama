using Nakama.Shared.Domain;

namespace Nakama.Users.Domain;

public partial class User : AggregateRoot<UserId>
{
    protected User()
    {
    }

    public virtual string Email { get; private set; } = string.Empty;

    public virtual string NormalizedEmail { get; private set; } = string.Empty;

    public virtual string Name { get; private set; } = string.Empty;

    public virtual string? PasswordHash { get; private set; }

    public virtual DateTime? LastActiveAt { get; private set; }

    public virtual DateTime CreatedAt { get; private set; }

    // Not virtual, unlike every property above: NHibernate only needs virtual to generate a
    // lazy-loading proxy, and Lazy(false) (every ClassMapping<T> in this codebase) disables
    // proxy generation entirely - ADR-17's own reasoning for not retrofitting virtual onto
    // domain methods applies just as much to a plain property. Scoped to this one property,
    // not a retroactive cleanup of the ones above.
    public bool IsAdmin { get; private set; }
}
