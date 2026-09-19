using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.StatusUpdater;

public static class ChatbotStatusUpdater
{
    public sealed record Command(Guid Id, ChatbotStatus Status) : ICommand;

    public sealed class Handler(IChatbotsRepository chatbotsRepository, IEventBus eventBus)
        : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            var key = ChatbotId.From(cmd.Id);
            var chatbot = await chatbotsRepository.FindById(key, ct)
                          ?? throw ChatbotErrors.NotFound(cmd.Id.ToString());

            chatbot.UpdateStatus(cmd.Status);

            await chatbotsRepository.Save(chatbot, ct);
            await eventBus.Publish(chatbot, ct);
        }
    }
}