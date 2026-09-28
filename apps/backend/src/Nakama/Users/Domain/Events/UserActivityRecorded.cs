using Nakama.Shared.Domain;

namespace Nakama.Users.Domain.Events;

public sealed record UserActivityRecorded(UserId Id, DateTime OccurredAt) : IDomainEvent;
