using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.TeamMemberAdder;

public static class ChatbotTeamMemberAdder
{
    public sealed record Command(Guid Id, string Name, TeamRole Role) : ICommand;

    public sealed class Handler(Adder adder) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await adder.Execute(ChatbotId.From(cmd.Id), cmd.Name, cmd.Role, ct);
        }
    }

    public sealed class Adder(IChatbotsRepository repository, IEventBus eventBus) : IService
    {
        private readonly ChatbotFinderById _finder = new(repository);

        public async Task Execute(ChatbotId id, string name, TeamRole role, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await _finder.Execute(id, cancellationToken);

            chatbot.AddTeamMember(new TeamMember(name, role));

            await repository.Save(chatbot, cancellationToken);
            await eventBus.Publish(chatbot, cancellationToken);
        }
    }
}
