using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.DetailsUpdater;

public static class FormationDetailsUpdater
{
    public sealed record Command(Guid Id, string Title, string Audience, string Description) : ICommand;

    public sealed class Handler(Updater updater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await updater.Execute(FormationId.From(cmd.Id), cmd.Title, cmd.Audience, cmd.Description, ct);
        }
    }

    public sealed class Updater(IFormationsRepository repository, IEventBus eventBus) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(FormationId id, string title, string audience, string description, CancellationToken cancellationToken)
        {
            Formation formation = await this._finder.Execute(id, cancellationToken);

            formation.UpdateDetails(title, audience, description);

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
