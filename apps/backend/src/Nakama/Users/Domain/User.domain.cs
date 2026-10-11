using Nakama.Shared.Domain;
using Nakama.Users.Domain.Events;

namespace Nakama.Users.Domain;

public partial class User
{
    private User(UserId key, string email, string normalizedEmail, string name, bool isAdmin, DateTime createdAt)
    {
        this.Id = key;
        this.Email = email;
        this.NormalizedEmail = normalizedEmail;
        this.Name = name;
        this.IsAdmin = isAdmin;
        this.CreatedAt = createdAt;
    }

    public static User Create(UserId key, Email email, string name, bool isAdmin, ISystemClock systemClock)
    {
        User user = new(key, email.Value, email.Normalized, name, isAdmin, systemClock.UtcNow);
        user.Record(new UserCreated(key, email.Value, name));
        return user;
    }

    public void SetPasswordHash(string? hash) => this.PasswordHash = hash;

    public void SetEmail(string email) => this.Email = email;

    public void SetNormalizedEmail(string normalizedEmail) => this.NormalizedEmail = normalizedEmail;

    public void MarkActive(DateTime now)
    {
        this.LastActiveAt = now;
        this.Record(new UserActivityRecorded(this.Id, now));
    }
}
