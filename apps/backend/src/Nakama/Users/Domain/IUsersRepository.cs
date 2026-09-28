namespace Nakama.Users.Domain;

public sealed record UserCredentialResult(bool Succeeded, IReadOnlyList<string> Errors);

public interface IUsersRepository
{
    Task<User?> FindById(UserId id, CancellationToken ct);

    Task<User?> GetByEmail(Email email, CancellationToken ct);

    Task Save(User user, CancellationToken ct);

    Task<int> CountAll(CancellationToken ct);

    Task<int> CountActiveSince(DateTime cutoff, CancellationToken ct);

    Task<UserCredentialResult> CreateWithPassword(User user, string password, CancellationToken ct);

    Task<bool> VerifyPassword(User user, string password, CancellationToken ct);
}
