using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.DocumentAdder;

public static class FormationDocumentAdder
{
    public sealed record Command(Guid Id, Guid DocumentId, string Name, string Origin, string OriginalFileName, Stream Content, long Length) : ICommand;

    public sealed class Handler(Adder adder) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await adder.Execute(FormationId.From(cmd.Id), cmd.DocumentId, cmd.Name, cmd.Origin, cmd.OriginalFileName, cmd.Content, cmd.Length, ct);
        }
    }

    public sealed class Adder(
        IFormationsRepository repository,
        IFormationDocumentStorage documentStorage,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(FormationId id, Guid documentId, string name, string origin, string originalFileName,
            Stream content, long length, CancellationToken cancellationToken)
        {
            Formation formation = await this._finder.Execute(id, cancellationToken);

            string fileName = await documentStorage.Save(documentId, originalFileName, content, length, cancellationToken);

            formation.AddDocument(new FormationDocument(documentId, name, origin, systemClock.UtcNow, fileName));

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
