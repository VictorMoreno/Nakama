using Microsoft.AspNetCore.Identity;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using Nakama.Users.Domain;
using NHibernate;
using NHibernate.Linq;

namespace Nakama.Users.Infrastructure;

public sealed class NhUsersRepository(IWriteSessionFactory writeSessions, UserManager<User> userManager)
    : NhRepositoryBase<User, UserId>(writeSessions), IUsersRepository
{
    public async Task<UserCredentialResult> CreateWithPassword(User user, string password, CancellationToken ct)
    {
        IdentityResult result = await userManager.CreateAsync(user, password);
        return new UserCredentialResult(result.Succeeded, result.Errors.Select(e => e.Description).ToList());
    }

    public Task<bool> VerifyPassword(User user, string password, CancellationToken ct) =>
        userManager.CheckPasswordAsync(user, password);


    public async Task<User?> GetByEmail(Email email, CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<User>().SingleOrDefaultAsync(u => u.NormalizedEmail == email.Normalized, ct);
    }

    public async Task<IReadOnlyList<User>> FindAll(CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<User>().OrderBy(u => u.Name).ToListAsync(ct);
    }

    public async Task<int> CountAll(CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<User>().CountAsync(ct);
    }

    public async Task<int> CountActiveSince(DateTime cutoff, CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<User>().CountAsync(u => u.LastActiveAt != null && u.LastActiveAt >= cutoff, ct);
    }
}
