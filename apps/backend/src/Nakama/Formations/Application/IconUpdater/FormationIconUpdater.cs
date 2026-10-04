using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.IconUpdater;

public static class FormationIconUpdater
{
    public sealed record Upload(string ContentType, Stream Content, long Length);

    public sealed record Command(Guid Id, Upload? Upload) : ICommand;

    public sealed class Handler(Updater updater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await updater.Execute(FormationId.From(cmd.Id), cmd.Upload, ct);
        }
    }

    public sealed class Updater(
        IFormationsRepository repository,
        IFormationIconStorage iconStorage,
        IEventBus eventBus) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(FormationId id, Upload? upload, CancellationToken cancellationToken)
        {
            Formation formation = await this._finder.Execute(id, cancellationToken);

            string? iconFileName = null;
            if (upload is not null)
            {
                iconFileName = await iconStorage.Save(id.Value, upload.ContentType, upload.Content, upload.Length, cancellationToken);
            }
            else
            {
                iconStorage.DeleteExisting(id.Value);
            }

            formation.UpdateIcon(iconFileName);

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
