namespace Nakama.Shared.Domain;

public interface ICommandBus
{
    Task Send(ICommand command, CancellationToken ct);
}
