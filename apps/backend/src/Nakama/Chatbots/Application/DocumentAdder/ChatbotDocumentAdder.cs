using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.DocumentAdder;

public static class ChatbotDocumentAdder
{
    public sealed record Command(Guid Id, string Name, string Origin, DocStatus Status, string StatusLabel) : ICommand;

    public sealed class Handler(
        IChatbotsRepository chatbotsRepository,
        IEventBus eventBus,
        ISystemClock systemClock) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            var key = ChatbotId.From(cmd.Id);
            var chatbot = await chatbotsRepository.FindById(key, ct)
                ?? throw ChatbotErrors.NotFound(cmd.Id.ToString());

            chatbot.AddDocument(new ChatbotDocument(cmd.Name, cmd.Origin, cmd.Status, cmd.StatusLabel, systemClock.UtcNow));

            await chatbotsRepository.Save(chatbot, ct);
            await eventBus.Publish(chatbot, ct);
        }
    }
}
