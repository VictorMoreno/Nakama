using MediatR;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.Archiver;

public static class ChatbotArchiver
{
    public sealed record Command(string Key) : IRequest<Response>;

    public sealed record Response;

    public sealed class Handler(IChatbotsRepository chatbotsRepository, IEventBus eventBus) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command cmd, CancellationToken ct)
        {
            var key = ChatbotKey.From(cmd.Key);
            var chatbot = await chatbotsRepository.FindByKey(key, ct)
                ?? throw ChatbotErrors.NotFound(cmd.Key);

            chatbot.Archive();

            await chatbotsRepository.Save(chatbot, ct);
            await eventBus.Publish([chatbot], ct);

            return new Response();
        }
    }
}
