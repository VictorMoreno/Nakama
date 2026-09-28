namespace Nakama.Shared.Domain;

public abstract class AggregateRoot : IEntity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents => this._domainEvents;

    protected void Record(IDomainEvent @event) => this._domainEvents.Add(@event);

    public void ClearDomainEvents() => this._domainEvents.Clear();
}

public abstract class AggregateRoot<TId> : AggregateRoot
{
    public TId Id { get; protected set; } = default!;
}
