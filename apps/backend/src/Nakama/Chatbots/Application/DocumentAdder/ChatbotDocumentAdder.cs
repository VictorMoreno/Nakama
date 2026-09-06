using MediatR;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.DocumentAdder;

public static class ChatbotDocumentAdder
{
    public sealed record Command(string Key, string Name, string Origin, DocStatus Status, string StatusLabel) : IRequest<Response>;

    public sealed record Response;

    public sealed class Handler(
        IChatbotsRepository chatbotsRepository,
        IEventBus eventBus,
        ISystemClock systemClock) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command cmd, CancellationToken ct)
        {
            var key = ChatbotKey.From(cmd.Key);
            var chatbot = await chatbotsRepository.FindByKey(key, ct)
                ?? throw ChatbotErrors.NotFound(cmd.Key);

            chatbot.AddDocument(new ChatbotDocument(cmd.Name, cmd.Origin, cmd.Status, cmd.StatusLabel, systemClock.UtcNow));

            await chatbotsRepository.Save(chatbot, ct);
            await eventBus.Publish([chatbot], ct);

            return new Response();
        }
    }
}
