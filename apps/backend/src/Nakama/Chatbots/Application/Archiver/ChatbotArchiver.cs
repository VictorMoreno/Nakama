using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.Archiver;

public static class ChatbotArchiver
{
    public sealed record Command(Guid Id) : ICommand;

    public sealed class Handler(IChatbotsRepository chatbotsRepository, IEventBus eventBus) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            var key = ChatbotId.From(cmd.Id);
            var chatbot = await chatbotsRepository.FindById(key, ct)
                ?? throw ChatbotErrors.NotFound(cmd.Id.ToString());

            chatbot.Archive();

            await chatbotsRepository.Save(chatbot, ct);
            await eventBus.Publish(chatbot, ct);
        }
    }
}
