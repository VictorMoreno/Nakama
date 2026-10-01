using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain.Events;

public sealed record PersonUnenrolled(Guid FormationId, Guid UserId) : IDomainEvent;
