using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.DocumentRemover;

public static class FormationDocumentRemover
{
    public sealed record Command(Guid Id, Guid DocumentId) : ICommand;

    public sealed class Handler(Remover remover) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await remover.Execute(FormationId.From(cmd.Id), cmd.DocumentId, ct);
        }
    }

    public sealed class Remover(
        IFormationsRepository repository,
        IFormationDocumentStorage documentStorage,
        IEventBus eventBus) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(FormationId id, Guid documentId, CancellationToken cancellationToken)
        {
            Formation formation = await this._finder.Execute(id, cancellationToken);

            string? fileName = formation.Documents.FirstOrDefault(document => document.Id == documentId)?.FileName;

            formation.RemoveDocument(documentId);

            if (fileName is not null)
            {
                documentStorage.Delete(fileName);
            }

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
