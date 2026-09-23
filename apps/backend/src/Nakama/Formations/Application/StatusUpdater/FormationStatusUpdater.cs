using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.StatusUpdater;

public static class FormationStatusUpdater
{
    public sealed record Command(Guid Id, int Status) : ICommand;

    public sealed class Handler(Updater updater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await updater.Execute(FormationId.From(cmd.Id), (FormationStatus)cmd.Status, ct);
        }
    }

    public sealed class Updater(IFormationsRepository repository, IEventBus eventBus) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(FormationId id, FormationStatus status, CancellationToken cancellationToken)
        {
            Formation formation = await _finder.Execute(id, cancellationToken);

            formation.UpdateStatus(status);

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
