using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.Creator;

public static class ChatbotCreator
{
    public sealed record Command(
        Guid Id,
        string Label,
        string Description,
        ChatbotStatus Status,
        bool General,
        ChatbotSettings Settings) : ICommand;

    public sealed class Handler(
        IChatbotsRepository chatbotsRepository,
        IEventBus eventBus,
        ISystemClock systemClock) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            if (await chatbotsRepository.FindById(ChatbotId.From(cmd.Id), ct) is not null)
            {
                throw ChatbotErrors.AlreadyExists(cmd.Id.ToString());
            }

            var chatbot = Chatbot.Create(
                ChatbotId.From(cmd.Id),
                cmd.Label,
                cmd.Description,
                cmd.Status,
                cmd.General,
                cmd.Settings,
                systemClock);

            await chatbotsRepository.Save(chatbot, ct);
            await eventBus.Publish(chatbot, ct);
        }
    }
}
