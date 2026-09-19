using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.StatusUpdater;

public static class FormationStatusUpdater
{
    public sealed record Command(Guid Id, FormationStatus Status) : ICommand;

    public sealed class Handler(IFormationsRepository formationsRepository, IEventBus eventBus) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            var key = FormationId.From(cmd.Id);
            var formation = await formationsRepository.FindById(key, ct)
                ?? throw FormationErrors.NotFound(cmd.Id.ToString());

            formation.UpdateStatus(cmd.Status);

            await formationsRepository.Save(formation, ct);
            await eventBus.Publish(formation, ct);
        }
    }
}
