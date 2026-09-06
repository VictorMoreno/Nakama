using MediatR;
using Nakama.Chatbots.Domain;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.Creator;

public static class ChatbotCreator
{
    public sealed record Command(
        string Label,
        string Description,
        ChatbotStatus Status,
        bool General,
        ChatbotSettings Settings) : IRequest<Response>;

    public sealed record Response(string Key);

    public sealed class Handler(
        IChatbotsRepository chatbotsRepository,
        IEventBus eventBus,
        IUuidGenerator uuidGenerator,
        ISystemClock systemClock) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command cmd, CancellationToken ct)
        {
            var chatbot = Chatbot.Create(
                ChatbotKey.From(uuidGenerator.NewId()),
                cmd.Label,
                cmd.Description,
                cmd.Status,
                cmd.General,
                cmd.Settings,
                systemClock);

            await chatbotsRepository.Save(chatbot, ct);
            await eventBus.Publish([chatbot], ct);

            return new Response(chatbot.Key.Value);
        }
    }
}
