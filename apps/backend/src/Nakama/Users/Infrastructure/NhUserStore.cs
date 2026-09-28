using Microsoft.AspNetCore.Identity;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Users.Domain;
using NHibernate.Linq;

namespace Nakama.Users.Infrastructure;

public sealed class NhUserStore(IWriteSessionFactory writeSessions) : IUserStore<User>, IUserPasswordStore<User>, IUserEmailStore<User>
{
    public Task<string> GetUserIdAsync(User user, CancellationToken ct) => Task.FromResult(user.Id.Value.ToString());

    public Task<string?> GetUserNameAsync(User user, CancellationToken ct) => Task.FromResult<string?>(user.Email);

    public Task SetUserNameAsync(User user, string? userName, CancellationToken ct)
    {
        user.SetEmail(userName ?? string.Empty);
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken ct) => Task.FromResult<string?>(user.NormalizedEmail);

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken ct)
    {
        user.SetNormalizedEmail(normalizedName ?? string.Empty);
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken ct)
    {
        await this.Save(user, ct);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken ct)
    {
        await this.Save(user, ct);
        return IdentityResult.Success;
    }

    public Task<IdentityResult> DeleteAsync(User user, CancellationToken ct) =>
        throw new NotSupportedException("User deletion is not supported yet.");

    public async Task<User?> FindByIdAsync(string userId, CancellationToken ct)
    {
        using var session = writeSessions.OpenSession();
        return await session.GetAsync<User>(UserId.From(Guid.Parse(userId)), ct);
    }

    public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken ct) =>
        this.GetByNormalizedEmail(normalizedUserName, ct);

    public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken ct)
    {
        user.SetPasswordHash(passwordHash);
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(User user, CancellationToken ct) => Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(User user, CancellationToken ct) => Task.FromResult(user.PasswordHash is not null);

    public Task SetEmailAsync(User user, string? email, CancellationToken ct)
    {
        user.SetEmail(email ?? string.Empty);
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(User user, CancellationToken ct) => Task.FromResult<string?>(user.Email);

    // No email-confirmation flow yet - admin-provisioned users are implicitly confirmed.
    public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken ct) => Task.FromResult(true);

    public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken ct) => Task.CompletedTask;

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken ct) =>
        this.GetByNormalizedEmail(normalizedEmail, ct);

    public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken ct) => Task.FromResult<string?>(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken ct)
    {
        user.SetNormalizedEmail(normalizedEmail ?? string.Empty);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    private async Task Save(User user, CancellationToken ct)
    {
        using var session = writeSessions.OpenSession();
        using var tx = session.BeginTransaction();
        await session.SaveOrUpdateAsync(user, ct);
        await tx.CommitAsync(ct);
    }

    private async Task<User?> GetByNormalizedEmail(string normalizedEmail, CancellationToken ct)
    {
        using var session = writeSessions.OpenSession();
        return await session.Query<User>().SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, ct);
    }
}
