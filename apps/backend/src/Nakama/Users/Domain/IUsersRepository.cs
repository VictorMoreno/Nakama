using Nakama.Shared.Domain.Specification;

namespace Nakama.Users.Domain;

public interface IUsersRepository
{
    Task<User?> FindById(UserId id, CancellationToken ct);

    Task<IReadOnlyList<User>> Search(ISpecification<User> specification, CancellationToken ct);

    Task<int> Count(ISpecification<User> specification, CancellationToken ct);

    Task Save(User user, CancellationToken ct);

    Task<bool> VerifyPassword(User user, string password);
}
