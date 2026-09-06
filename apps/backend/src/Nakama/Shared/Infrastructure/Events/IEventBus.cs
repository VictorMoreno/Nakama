using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Events;

/// <summary>
/// Publishes every pending domain event raised by the given aggregates, then clears them.
/// Invoked explicitly at the end of each use-case Handler, after the write-side transaction
/// has committed - NHibernate has no <c>SaveChangesInterceptor</c>-style hook, and this scope
/// has no outbox, so dispatch is synchronous within the same request.
/// </summary>
public interface IEventBus
{
    Task Publish(IEnumerable<AggregateRoot> aggregates, CancellationToken ct);
}
