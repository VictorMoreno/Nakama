using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Application;
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

    public sealed class Handler(Creator creator) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await creator.Execute(ChatbotId.From(cmd.Id), cmd, ct);
        }
    }

    public sealed class Creator(
        IChatbotsRepository repository,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        // No finder here: this is the inverse check (must NOT already exist), a
        // NotFound-throwing ChatbotFinderById doesn't fit this shape.
        public async Task Execute(ChatbotId id, Command cmd, CancellationToken cancellationToken)
        {
            if (await repository.FindById(id, cancellationToken) is not null)
            {
                throw ChatbotErrors.AlreadyExists(id.ToString());
            }

            var chatbot = Chatbot.Create(
                id,
                cmd.Label,
                cmd.Description,
                cmd.Status,
                cmd.General,
                cmd.Settings,
                systemClock);

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}
