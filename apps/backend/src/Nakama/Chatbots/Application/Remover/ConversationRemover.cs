using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
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
        // 404 for both "doesn't exist" and "exists but belongs to someone else" - same message
        // either way, so a caller can't use this to probe which conversation ids are real.
        public async Task Execute(ConversationId id, Guid userId, CancellationToken cancellationToken)
        {
            var conversation = await repository.FindById(id, cancellationToken);
            if (conversation is null || conversation.UserId != userId)
            {
                throw ConversationErrors.NotFound(id);
            }

            conversation.Delete();

            await repository.Remove(conversation, cancellationToken);
            await eventBus.Publish(conversation, cancellationToken);
        }
    }
}
