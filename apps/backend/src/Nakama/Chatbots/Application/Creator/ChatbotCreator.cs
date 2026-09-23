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
        int Status,
        bool General,
        bool Socratic,
        bool ExplicaMas,
        int Provider) : ICommand;

    public sealed class Handler(Creator creator) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await creator.Execute(
                ChatbotId.From(cmd.Id),
                cmd.Label,
                cmd.Description,
                (ChatbotStatus)cmd.Status,
                cmd.General,
                new ChatbotSettings(cmd.Socratic, cmd.ExplicaMas, (LlmProvider)cmd.Provider),
                ct);
        }
    }

    public sealed class Creator(
        IChatbotsRepository repository,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        public async Task Execute(
            ChatbotId id,
            string label,
            string description,
            ChatbotStatus status,
            bool general,
            ChatbotSettings settings,
            CancellationToken cancellationToken)
        {
            if (await repository.FindById(id, cancellationToken) is not null)
            {
                throw ChatbotErrors.AlreadyExists(id.ToString());
            }

            var chatbot = Chatbot.Create(id, label, description, status, general, settings, systemClock);

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}
