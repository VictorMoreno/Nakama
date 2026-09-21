using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Application;
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

    public sealed class Handler(Creator creator) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await creator.Execute(FormationId.From(cmd.Id), cmd, ct);
        }
    }

    public sealed class Creator(IFormationsRepository repository, IEventBus eventBus) : IService
    {
        public async Task Execute(FormationId id, Command cmd, CancellationToken cancellationToken)
        {
            if (await repository.FindById(id, cancellationToken) is not null)
            {
                throw FormationErrors.AlreadyExists(id.ToString());
            }

            var formation = Formation.Create(
                id,
                cmd.Title,
                cmd.Audience,
                cmd.Description,
                cmd.Status,
                cmd.LinkedChatbotKey);

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
