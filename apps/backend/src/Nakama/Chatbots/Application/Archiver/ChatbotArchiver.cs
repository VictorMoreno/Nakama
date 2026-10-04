using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.Archiver;

public static class ChatbotArchiver
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class Handler(Archiver archiver) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await archiver.Execute(ChatbotId.From(cmd.Id), ct);
        }
    }

    public sealed class Archiver(IChatbotsRepository repository, IEventBus eventBus) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await this._finder.Execute(id, cancellationToken);

            chatbot.Archive();

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}
