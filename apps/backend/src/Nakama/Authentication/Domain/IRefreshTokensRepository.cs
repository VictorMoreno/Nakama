using Nakama.Shared.Domain.Specification;

namespace Nakama.Authentication.Domain;

public interface IRefreshTokensRepository
{
    Task<IReadOnlyList<RefreshToken>> Search(ISpecification<RefreshToken> specification, CancellationToken ct);

    Task Save(RefreshToken token, CancellationToken ct);

    Task<bool> Rotate(RefreshToken current, RefreshToken replacement, DateTime now, CancellationToken ct);

    Task RevokeAllOf(Guid userId, DateTime now, CancellationToken ct);
}
