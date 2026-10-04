using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.ModuleGatingToggler;

public static class FormationModuleGatingToggler
{
    public sealed record Command(Guid Id, Guid ModuleId) : ICommand;

    public sealed class Handler(Toggler toggler) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await toggler.Execute(FormationId.From(cmd.Id), cmd.ModuleId, ct);
        }
    }

    public sealed class Toggler(IFormationsRepository repository, IEventBus eventBus) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(FormationId id, Guid moduleId, CancellationToken cancellationToken)
        {
            Formation formation = await this._finder.Execute(id, cancellationToken);

            formation.ToggleModuleGating(Domain.ModuleId.From(moduleId));

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
