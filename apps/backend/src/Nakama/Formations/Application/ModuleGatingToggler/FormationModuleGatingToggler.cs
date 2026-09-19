using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.ModuleGatingToggler;

public static class FormationModuleGatingToggler
{
    public sealed record Command(Guid Id, Guid ModuleId) : ICommand;

    public sealed class Handler(IFormationsRepository formationsRepository, IEventBus eventBus) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            var key = FormationId.From(cmd.Id);
            var formation = await formationsRepository.FindById(key, ct)
                ?? throw FormationErrors.NotFound(cmd.Id.ToString());

            formation.ToggleModuleGating(Domain.ModuleId.From(cmd.ModuleId));

            await formationsRepository.Save(formation, ct);
            await eventBus.Publish(formation, ct);
        }
    }
}
