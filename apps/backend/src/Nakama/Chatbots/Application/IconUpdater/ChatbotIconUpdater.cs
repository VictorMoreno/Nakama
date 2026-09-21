using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.IconUpdater;

public static class ChatbotIconUpdater
{
    public sealed record Command(Guid Id, string? IconFileName) : ICommand;

    public sealed class Handler(Updater updater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await updater.Execute(ChatbotId.From(cmd.Id), cmd.IconFileName, ct);
        }
    }

    public sealed class Updater(IChatbotsRepository repository, IEventBus eventBus) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, string? iconFileName, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await _finder.Execute(id, cancellationToken);

            chatbot.UpdateIcon(iconFileName);

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}
