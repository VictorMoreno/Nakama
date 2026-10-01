using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Domain.Events;

// CompletedModuleCount travels with the event (not just the one ModuleId just completed) so
// read-model consumers (FormationDetail's team panel %) don't need a second round-trip into
// FormationEnrollment to know the running total.
public sealed record EnrollmentModuleCompleted(Guid FormationId, Guid UserId, Guid ModuleId, int? Score, int CompletedModuleCount) : IDomainEvent;
