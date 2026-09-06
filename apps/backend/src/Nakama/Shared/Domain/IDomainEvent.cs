namespace Nakama.Shared.Domain;

/// <summary>
/// Marker for a domain fact raised by an aggregate. Intentionally has no
/// reference to MediatR or any other infrastructure concern - the domain
/// only knows it raised a fact, never how it gets dispatched.
/// </summary>
public interface IDomainEvent
{
}
