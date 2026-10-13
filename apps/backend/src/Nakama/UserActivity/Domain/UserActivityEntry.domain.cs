namespace Nakama.UserActivity.Domain;

public partial class UserActivityEntry
{
    public UserActivityEntry(Guid id, Guid userId, string type, DateTime occurredAt)
    {
        this.Id = id;
        this.UserId = userId;
        this.Type = type;
        this.OccurredAt = occurredAt;
    }

    public static UserActivityEntry Create(Guid id, Guid userId, string type, DateTime occurredAt) =>
        new(id, userId, type, occurredAt);
}
