using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.Archiver;

public static class FormationArchiver
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class Handler(Archiver archiver) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await archiver.Execute(FormationId.From(cmd.Id), ct);
        }
    }

    public sealed class Archiver(IFormationsRepository repository, IEventBus eventBus) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(FormationId id, CancellationToken cancellationToken)
        {
            Formation formation = await _finder.Execute(id, cancellationToken);

            formation.Archive();

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
