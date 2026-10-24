using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationDocumentRemoved(FormationId Id, Guid DocumentId) : IDomainEvent;
