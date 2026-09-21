using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.IconUpdater;

public static class FormationIconUpdater
{
    public sealed record Command(Guid Id, string? IconFileName) : ICommand;

    public sealed class Handler(Updater updater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await updater.Execute(FormationId.From(cmd.Id), cmd.IconFileName, ct);
        }
    }

    public sealed class Updater(IFormationsRepository repository, IEventBus eventBus) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(FormationId id, string? iconFileName, CancellationToken cancellationToken)
        {
            Formation formation = await _finder.Execute(id, cancellationToken);

            formation.UpdateIcon(iconFileName);

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
