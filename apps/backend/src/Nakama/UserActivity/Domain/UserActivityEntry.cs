using Nakama.Shared.Domain;

namespace Nakama.UserActivity.Domain;

public partial class UserActivityEntry : IEntity
{
    protected UserActivityEntry() { }

    public Guid Id { get; protected set; }
    public Guid UserId { get; protected set; }
    public string Type { get; protected set; } = string.Empty;
    public DateTime OccurredAt { get; protected set; }
}
