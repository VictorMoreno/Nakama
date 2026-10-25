using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.DetailsUpdater;

public static class ChatbotDetailsUpdater
{
    public sealed record Command(Guid Id, string Label, string Description) : ICommand;

    public sealed class Handler(Updater updater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await updater.Execute(ChatbotId.From(cmd.Id), cmd.Label, cmd.Description, ct);
        }
    }

    public sealed class Updater(IChatbotsRepository repository, IEventBus eventBus) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, string label, string description, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await this._finder.Execute(id, cancellationToken);

            chatbot.UpdateDetails(label, description);

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}
