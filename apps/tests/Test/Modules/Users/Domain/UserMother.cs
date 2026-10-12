using Nakama.Users.Domain;

namespace Test.Modules.Users.Domain;

public static class UserMother
{
    public static User Create(
        UserId id,
        Email email,
        string name,
        string? passwordHash,
        DateTime? lastActiveAt,
        DateTime createdAt,
        bool isAdmin) =>
        new(id, email.Value, email.Normalized, name, passwordHash, lastActiveAt, createdAt, isAdmin);

    public static User Random() => With();

    public static User WithAdmin() => With(isAdmin: true);

    public static User WithId(UserId id) => With(id: id);

    public static User WithEmail(Email email) => With(email: email);

    public static UserId RandomId() => UserId.From(Guid.NewGuid());

    public static Email RandomEmail() => new($"{Guid.NewGuid():N}@nakama.test");

    public static User With(
        UserId? id = null,
        Email? email = null,
        string? name = null,
        string? passwordHash = null,
        DateTime? lastActiveAt = null,
        DateTime? createdAt = null,
        bool? isAdmin = null) =>
        Create(
            id ?? RandomId(),
            email ?? RandomEmail(),
            name ?? "Test User",
            passwordHash,
            lastActiveAt,
            createdAt ?? DateTime.UtcNow,
            isAdmin ?? false);
}
