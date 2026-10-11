using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.Remover;

public static class ConversationRemover
{
    public sealed record Command(Guid ConversationId, Guid UserId) : ICommand;

    public sealed class Handler(Remover remover) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await remover.Execute(ConversationId.From(cmd.ConversationId), cmd.UserId, ct);
        }
    }

    public sealed class Remover(IConversationsRepository repository, IEventBus eventBus) : IService
    {
        private readonly ConversationFinderById _finder = new(repository);

        public async Task Execute(ConversationId id, Guid userId, CancellationToken cancellationToken)
        {
            Conversation conversation = await this._finder.Execute(id, userId, cancellationToken);

            conversation.Delete();

            await repository.Remove(conversation, cancellationToken);
            await eventBus.Publish(conversation, cancellationToken);
        }
    }
}
