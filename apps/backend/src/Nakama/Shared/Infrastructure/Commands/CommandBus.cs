using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Commands;

public sealed class CommandBus(IServiceProvider serviceProvider) : ICommandBus
{
    public Task Send(ICommand command, CancellationToken ct)
    {
        // ICommandHandler<TCommand> is resolved directly from the container instead of routing
        // through MediatR: MediatR's open-generic handler resolution only substitutes generic
        // parameters positionally, so an open generic handler whose interface wraps its own
        // parameter in another type (e.g. IRequestHandler<CommandEnvelope<TCommand>>) can never
        // be resolved this way - verified at runtime, not just assumed (see ADR-13c). Reflection
        // here mirrors EventBus.WrapAsNotification: the concrete TCommand type is only known at
        // runtime, so the closed ICommandHandler<TCommand> is built via MakeGenericType.
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No {handlerType} registered for {command.GetType()}.");
        var handleMethod = handlerType.GetMethod(nameof(ICommandHandler<ICommand>.Handle))!;
        return (Task)handleMethod.Invoke(handler, [command, ct])!;
    }
}
