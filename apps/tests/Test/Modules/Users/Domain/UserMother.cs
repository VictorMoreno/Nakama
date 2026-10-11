using Nakama.Users.Domain;

namespace Test.Modules.Users.Domain;

public static class UserMother
{
    public static User Create(UserId id, Email email, string name, bool isAdmin, DateTime createdAt) =>
        new(id, email.Value, email.Normalized, name, isAdmin, createdAt);

    public static User Random() => With();

    public static User WithId(UserId id) => With(id: id);

    public static User WithEmail(Email email) => With(email: email);

    public static UserId RandomId() => UserId.From(Guid.NewGuid());

    public static Email RandomEmail() => new($"{Guid.NewGuid():N}@nakama.test");

    public static User With(
        UserId? id = null,
        Email? email = null,
        string? name = null,
        bool? isAdmin = null,
        DateTime? createdAt = null) =>
        Create(
            id ?? RandomId(),
            email ?? RandomEmail(),
            name ?? "Test User",
            isAdmin ?? false,
            createdAt ?? DateTime.UtcNow);
}
