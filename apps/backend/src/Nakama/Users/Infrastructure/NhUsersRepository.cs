using Microsoft.AspNetCore.Identity;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using Nakama.Users.Domain;

namespace Nakama.Users.Infrastructure;

public sealed class NhUsersRepository(IWriteSessionFactory writeSessions, UserManager<User> userManager)
    : NhRepositoryBase<User, UserId>(writeSessions), IUsersRepository
{
    public Task<bool> VerifyPassword(User user, string password) =>
        userManager.CheckPasswordAsync(user, password);
}
