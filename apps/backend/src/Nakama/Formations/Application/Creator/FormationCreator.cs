using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.Creator;

public static class FormationCreator
{
    public sealed record Command(
        Guid Id,
        string Title,
        string Audience,
        string Description,
        FormationStatus Status,
        Guid? LinkedChatbotKey) : ICommand;

    public sealed class Handler(
        IFormationsRepository formationsRepository,
        IEventBus eventBus) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            if (await formationsRepository.FindById(FormationId.From(cmd.Id), ct) is not null)
            {
                throw FormationErrors.AlreadyExists(cmd.Id.ToString());
            }

            var formation = Formation.Create(
                FormationId.From(cmd.Id),
                cmd.Title,
                cmd.Audience,
                cmd.Description,
                cmd.Status,
                cmd.LinkedChatbotKey);

            await formationsRepository.Save(formation, ct);
            await eventBus.Publish(formation, ct);
        }
    }
}
