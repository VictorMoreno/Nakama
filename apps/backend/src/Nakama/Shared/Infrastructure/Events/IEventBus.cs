using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Events;

public interface IEventBus
{
    Task Publish(AggregateRoot aggregate, CancellationToken ct);
}
