using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.ModuleCompleter;

public static class FormationModuleCompleter
{
    public sealed record Command(Guid Id, Guid ModuleId, int? Score) : ICommand;

    public sealed class Handler(Completer completer) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await completer.Execute(FormationId.From(cmd.Id), cmd.ModuleId, cmd.Score, ct);
        }
    }

    public sealed class Completer(IFormationsRepository repository, IEventBus eventBus) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(FormationId id, Guid moduleId, int? score, CancellationToken cancellationToken)
        {
            Formation formation = await _finder.Execute(id, cancellationToken);

            formation.CompleteModule(Domain.ModuleId.From(moduleId), score);

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
