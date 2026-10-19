using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.DocumentRemover;

public static class ChatbotDocumentRemover
{
    public sealed record Command(Guid Id, Guid DocumentId) : ICommand;

    public sealed class Handler(Remover remover) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await remover.Execute(ChatbotId.From(cmd.Id), cmd.DocumentId, ct);
        }
    }

    public sealed class Remover(
        IChatbotsRepository repository,
        IChatbotDocumentStorage documentStorage,
        IEventBus eventBus) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, Guid documentId, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await this._finder.Execute(id, cancellationToken);

            string? fileName = chatbot.Documents.FirstOrDefault(document => document.Id == documentId)?.FileName;

            chatbot.RemoveDocument(documentId);

            if (fileName is not null)
            {
                documentStorage.Delete(fileName);
            }

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}
