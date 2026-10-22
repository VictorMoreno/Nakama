using Microsoft.AspNetCore.Identity;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Users.Domain;
using NHibernate;
using NHibernate.Linq;

namespace Nakama.Users.Infrastructure;

public sealed class NhUserRepository(INhUnitOfWork unitOfWork) : IUserPasswordStore<User>, IUserEmailStore<User>
{
    public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.Id.Value.ToString());

    public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.Email);

    public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
    {
        user.SetEmail(userName ?? string.Empty);
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.NormalizedEmail);

    public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
    {
        user.SetNormalizedEmail(normalizedName ?? string.Empty);
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
    {
        await this.Save(user, cancellationToken);
        return IdentityResult.Success;
    }

    public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken) =>
        this.CreateAsync(user, cancellationToken);

    public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken) =>
        throw new NotSupportedException("User deletion is not supported yet.");

    public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await unitOfWork.Read(session => session.GetAsync<User>(UserId.From(Guid.Parse(userId)), cancellationToken));
    }

    public Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
        this.GetByNormalizedEmail(normalizedUserName, cancellationToken);

    public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.SetPasswordHash(passwordHash);
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash);

    public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.PasswordHash is not null);

    public Task SetEmailAsync(User user, string? email, CancellationToken cancellationToken)
    {
        user.SetEmail(email ?? string.Empty);
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(User user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.Email);

    // No email-confirmation flow yet - admin-provisioned users are implicitly confirmed.
    public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken) => Task.FromResult(true);

    public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<User?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        this.GetByNormalizedEmail(normalizedEmail, cancellationToken);

    public Task<string?> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken) => Task.FromResult<string?>(user.NormalizedEmail);

    public Task SetNormalizedEmailAsync(User user, string? normalizedEmail, CancellationToken cancellationToken)
    {
        user.SetNormalizedEmail(normalizedEmail ?? string.Empty);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    private async Task Save(User user, CancellationToken cancellationToken)
    {
        unitOfWork.EnsureTransaction();
        await unitOfWork.Session.SaveOrUpdateAsync(user, cancellationToken);
    }

    private async Task<User?> GetByNormalizedEmail(string normalizedEmail, CancellationToken cancellationToken)
    {
        return await unitOfWork.Read(session => session.Query<User>().SingleOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken));
    }
}
