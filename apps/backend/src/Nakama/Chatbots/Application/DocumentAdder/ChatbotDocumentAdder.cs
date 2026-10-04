using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.DocumentAdder;

public static class ChatbotDocumentAdder
{
    public sealed record Command(Guid Id, Guid DocumentId, string Name, string Origin, string FileName) : ICommand;

    public sealed class Handler(Adder adder) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await adder.Execute(ChatbotId.From(cmd.Id), cmd.DocumentId, cmd.Name, cmd.Origin, cmd.FileName, ct);
        }
    }

    public sealed class Adder(
        IChatbotsRepository repository,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, Guid documentId, string name, string origin, string fileName,
            CancellationToken cancellationToken)
        {
            Chatbot chatbot = await this._finder.Execute(id, cancellationToken);

            chatbot.AddDocument(new ChatbotDocument(
                documentId, name, origin, DocStatus.Pending, "Pendiente", systemClock.UtcNow, fileName));

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}