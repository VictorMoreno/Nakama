using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.DocumentRemover;

public static class ChatbotDocumentRemover
{
    public sealed record Command(Guid Id, int Index) : ICommand;

    public sealed class Handler(Remover remover) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await remover.Execute(ChatbotId.From(cmd.Id), cmd.Index, ct);
        }
    }

    public sealed class Remover(IChatbotsRepository repository, IEventBus eventBus) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, int index, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await _finder.Execute(id, cancellationToken);

            chatbot.RemoveDocument(index);

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}
