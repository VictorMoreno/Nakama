using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.LlmProviders.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.SettingsUpdater;

public static class ChatbotSettingsUpdater
{
    public sealed record Command(Guid Id, bool Socratic, bool ExplicaMas, int Provider, string Model) : ICommand;

    public sealed class Handler(Updater updater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await updater.Execute(
                ChatbotId.From(cmd.Id),
                new ChatbotSettings(cmd.Socratic, cmd.ExplicaMas, (LlmProvider)cmd.Provider, cmd.Model),
                ct);
        }
    }

    public sealed class Updater(IChatbotsRepository repository, IEventBus eventBus) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, ChatbotSettings settings, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await this._finder.Execute(id, cancellationToken);

            chatbot.UpdateSettings(settings);

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}