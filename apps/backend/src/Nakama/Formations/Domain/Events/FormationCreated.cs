using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Events;

public sealed record FormationCreated(FormationKey Key, string Title, string Audience, string Description, FormationStatus Status, string? LinkedChatbotKey) : IDomainEvent;
