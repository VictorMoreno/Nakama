namespace Nakama.Shared.Domain;

public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task Handle(TCommand command, CancellationToken ct);
}
