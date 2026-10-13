using Nakama.Shared.Domain;
using Nakama.Users.Domain.Events;

namespace Nakama.Users.Domain;

public partial class User
{
    public User(
        UserId key,
        string email,
        string normalizedEmail,
        string name,
        string? passwordHash,
        DateTime? lastActiveAt,
        DateTime createdAt,
        bool isAdmin)
    {
        this.Id = key;
        this.Email = email;
        this.NormalizedEmail = normalizedEmail;
        this.Name = name;
        this.PasswordHash = passwordHash;
        this.LastActiveAt = lastActiveAt;
        this.CreatedAt = createdAt;
        this.IsAdmin = isAdmin;
    }

    public static User Create(UserId key, Email email, string name, bool isAdmin, ISystemClock systemClock) =>
        new(key, email.Value, email.Normalized, name, null, null, systemClock.UtcNow, isAdmin);

    public void SetPasswordHash(string? hash) => this.PasswordHash = hash;

    public void SetEmail(string email) => this.Email = email;

    public void SetNormalizedEmail(string normalizedEmail) => this.NormalizedEmail = normalizedEmail;

    public void MarkActive(DateTime now)
    {
        this.LastActiveAt = now;
        this.Record(new UserActivityRecorded(this.Id, now));
    }
}
