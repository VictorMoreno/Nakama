namespace Nakama.Shared.Domain;

/// <summary>
/// Non-generic aggregate root base. Split from <see cref="AggregateRoot{TId}"/> so that
/// infrastructure code (namely <c>IEventBus</c>) can accept a heterogeneous
/// batch of aggregates - e.g. <c>IEnumerable&lt;AggregateRoot&gt;</c> - without being tied
/// to a single identity type.
/// </summary>
public abstract class AggregateRoot : IEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => this._domainEvents;

    protected void Raise(IDomainEvent @event) => this._domainEvents.Add(@event);

    public void ClearDomainEvents() => this._domainEvents.Clear();
}

/// <summary>
/// Aggregate root with a typed identity. <c>Id</c> uses <c>private set</c> + <c>virtual</c>
/// per agents.md §3: <c>virtual</c> is what NHibernate needs to generate lazy-load proxies,
/// the private setter keeps mutation confined to the aggregate's own behavior methods -
/// NHibernate still reaches it via reflection when hydrating.
/// </summary>
public abstract class AggregateRoot<TId> : AggregateRoot
{
    public virtual TId Id { get; protected set; } = default!;
}
