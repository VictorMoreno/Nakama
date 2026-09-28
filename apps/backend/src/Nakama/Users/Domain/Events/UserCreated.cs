using Nakama.Shared.Domain;

namespace Nakama.Users.Domain.Events;

public sealed record UserCreated(UserId Id, string Email, string Name) : IDomainEvent;
