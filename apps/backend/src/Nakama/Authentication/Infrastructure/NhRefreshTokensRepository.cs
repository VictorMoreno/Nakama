using Nakama.Authentication.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using NHibernate;
using NHibernate.Linq;

namespace Nakama.Authentication.Infrastructure;

public sealed class NhRefreshTokensRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<RefreshToken, RefreshTokenId>(writeSessions), IRefreshTokensRepository
{
    public async Task<RefreshToken?> FindByValue(string value, CancellationToken ct)
    {
        string tokenHash = RefreshToken.HashOf(value);
        using ISession session = this.Sessions.OpenSession();
        return await session.Query<RefreshToken>().SingleOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
    }

    public async Task<bool> Rotate(RefreshToken current, RefreshToken replacement, DateTime now, CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        using ITransaction tx = session.BeginTransaction();

        int rotated = await session
            .CreateSQLQuery("update refresh_tokens set revoked_at = :now, replaced_by_id = :replacementId where id = :id and revoked_at is null")
            .SetParameter("now", now)
            .SetParameter("replacementId", replacement.Id.Value)
            .SetParameter("id", current.Id.Value)
            .ExecuteUpdateAsync(ct);

        if (rotated == 0)
        {
            await tx.RollbackAsync(ct);
            return false;
        }

        await session.SaveAsync(replacement, ct);
        await tx.CommitAsync(ct);
        return true;
    }

    public async Task RevokeAllOf(Guid userId, DateTime now, CancellationToken ct)
    {
        using ISession session = this.Sessions.OpenSession();
        using ITransaction tx = session.BeginTransaction();

        await session
            .CreateSQLQuery("update refresh_tokens set revoked_at = :now where user_id = :userId and revoked_at is null")
            .SetParameter("now", now)
            .SetParameter("userId", userId)
            .ExecuteUpdateAsync(ct);

        await tx.CommitAsync(ct);
    }
}
