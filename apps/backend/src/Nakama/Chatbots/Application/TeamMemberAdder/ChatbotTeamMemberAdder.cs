using MediatR;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.TeamMemberAdder;

public static class ChatbotTeamMemberAdder
{
    public sealed record Command(string Key, string Name, TeamRole Role) : IRequest<Response>;

    public sealed record Response;

    public sealed class Handler(IChatbotsRepository chatbotsRepository, IEventBus eventBus) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command cmd, CancellationToken ct)
        {
            var key = ChatbotKey.From(cmd.Key);
            var chatbot = await chatbotsRepository.FindByKey(key, ct)
                ?? throw ChatbotErrors.NotFound(cmd.Key);

            chatbot.AddTeamMember(new TeamMember(cmd.Name, cmd.Role));

            await chatbotsRepository.Save(chatbot, ct);
            await eventBus.Publish([chatbot], ct);

            return new Response();
        }
    }
}
