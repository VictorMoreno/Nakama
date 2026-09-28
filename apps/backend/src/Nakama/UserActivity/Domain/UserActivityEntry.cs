using Nakama.Shared.Domain;

namespace Nakama.UserActivity.Domain;

public partial class UserActivityEntry : IEntity
{
    protected UserActivityEntry() { } // NHibernate

    public virtual Guid Id { get; protected set; }
    public virtual Guid UserId { get; protected set; }
    public virtual string Type { get; protected set; } = string.Empty;
    public virtual DateTime OccurredAt { get; protected set; }
}
