using Nakama.Shared.Domain;

namespace Nakama.Authentication.Domain.Events;

public sealed record SessionRefreshed(RefreshTokenId Id, Guid UserId, DateTime OccurredAt) : IDomainEvent;
