using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationDocumentAdded(FormationId Id, FormationDocument Document) : IDomainEvent;
