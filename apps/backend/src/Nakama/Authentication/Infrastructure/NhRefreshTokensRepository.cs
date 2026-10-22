using Nakama.Authentication.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.Authentication.Infrastructure;

public sealed class NhRefreshTokensRepository(INhUnitOfWork unitOfWork, INhUnitOfWorkFactory unitOfWorkFactory)
    : NhRepositoryBase<RefreshToken, RefreshTokenId>(unitOfWork), IRefreshTokensRepository
{
    public async Task<bool> Rotate(RefreshToken current, RefreshToken replacement, DateTime now, CancellationToken ct)
    {
        this.UnitOfWork.EnsureTransaction();

        int rotated = await this.UnitOfWork.Session
            .CreateSQLQuery("update refresh_tokens set revoked_at = :now, replaced_by_id = :replacementId where id = :id and revoked_at is null")
            .SetParameter("now", now)
            .SetParameter("replacementId", replacement.Id.Value)
            .SetParameter("id", current.Id.Value)
            .ExecuteUpdateAsync(ct);

        if (rotated == 0)
        {
            return false;
        }

        await this.UnitOfWork.Session.SaveAsync(replacement, ct);
        return true;
    }

    public async Task RevokeAllOf(Guid userId, DateTime now, CancellationToken ct)
    {
        await using INhUnitOfWork independentUnitOfWork = unitOfWorkFactory.Create();
        independentUnitOfWork.EnsureTransaction();

        await independentUnitOfWork.Session
            .CreateSQLQuery("update refresh_tokens set revoked_at = :now where user_id = :userId and revoked_at is null")
            .SetParameter("now", now)
            .SetParameter("userId", userId)
            .ExecuteUpdateAsync(ct);

        await independentUnitOfWork.SaveChanges(ct);
    }
}
