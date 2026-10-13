namespace Nakama.Users.Domain;

public interface IUsersRepository
{
    Task<User?> FindById(UserId id, CancellationToken ct);

    Task<IReadOnlyList<User>> FindAll(CancellationToken ct);

    Task<User?> GetByEmail(Email email, CancellationToken ct);

    Task Save(User user, CancellationToken ct);

    Task<int> CountAll(CancellationToken ct);

    Task<int> CountActiveSince(DateTime cutoff, CancellationToken ct);

    Task<bool> VerifyPassword(User user, string password, CancellationToken ct);
}
