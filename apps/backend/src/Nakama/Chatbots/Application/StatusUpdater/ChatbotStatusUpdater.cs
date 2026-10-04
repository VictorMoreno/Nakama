using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.StatusUpdater;

public static class ChatbotStatusUpdater
{
    public sealed record Command(Guid Id, int Status) : ICommand;

    public sealed class Handler(Updater updater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await updater.Execute(ChatbotId.From(cmd.Id), (ChatbotStatus)cmd.Status, ct);
        }
    }

    public sealed class Updater(IChatbotsRepository repository, IEventBus eventBus) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, ChatbotStatus status, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await this._finder.Execute(id, cancellationToken);

            chatbot.UpdateStatus(status);

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}