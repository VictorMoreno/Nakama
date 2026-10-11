using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.DocumentAdder;

public static class ChatbotDocumentAdder
{
    public sealed record Command(Guid Id, Guid DocumentId, string Name, string Origin, string OriginalFileName, Stream Content, long Length) : ICommand;

    public sealed class Handler(Adder adder) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await adder.Execute(ChatbotId.From(cmd.Id), cmd.DocumentId, cmd.Name, cmd.Origin, cmd.OriginalFileName, cmd.Content, cmd.Length, ct);
        }
    }

    public sealed class Adder(
        IChatbotsRepository repository,
        IChatbotDocumentStorage documentStorage,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, Guid documentId, string name, string origin, string originalFileName,
            Stream content, long length, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await this._finder.Execute(id, cancellationToken);

            string fileName = await documentStorage.Save(documentId, originalFileName, content, length, cancellationToken);

            chatbot.AddDocument(new ChatbotDocument(
                documentId, name, origin, DocStatus.Pending, "Pendiente", systemClock.UtcNow, fileName));

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}
