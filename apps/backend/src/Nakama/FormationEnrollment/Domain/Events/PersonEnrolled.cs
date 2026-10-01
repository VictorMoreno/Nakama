using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain.Events;

public sealed record PersonEnrolled(Guid FormationId, Guid UserId) : IDomainEvent;
