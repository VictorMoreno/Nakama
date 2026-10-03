using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Commands;

public sealed class CommandBus(IServiceProvider serviceProvider) : ICommandBus
{
    public Task Send(ICommand command, CancellationToken ct)
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        var handler = serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No {handlerType} registered for {command.GetType()}.");
        var handleMethod = handlerType.GetMethod(nameof(ICommandHandler<ICommand>.Handle))!;
        return (Task)handleMethod.Invoke(handler, [command, ct])!;
    }
}
