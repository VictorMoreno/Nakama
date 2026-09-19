using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.ModuleCompleter;

public static class FormationModuleCompleter
{
    public sealed record Command(Guid Id, string ModuleId, int? Score) : ICommand;

    public sealed class Handler(IFormationsRepository formationsRepository, IEventBus eventBus) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            var key = FormationId.From(cmd.Id);
            var formation = await formationsRepository.FindById(key, ct)
                ?? throw FormationErrors.NotFound(cmd.Id.ToString());

            formation.CompleteModule(Domain.ModuleId.From(cmd.ModuleId), cmd.Score);

            await formationsRepository.Save(formation, ct);
            await eventBus.Publish(formation, ct);
        }
    }
}
