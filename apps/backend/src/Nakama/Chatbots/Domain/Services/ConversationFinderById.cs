using Nakama.Chatbots.Domain.Errors;

namespace Nakama.Chatbots.Domain.Services;

public sealed class ConversationFinderById(IConversationsRepository repository)
{
    public async Task<Conversation> Execute(ConversationId id, Guid userId, CancellationToken cancellationToken)
    {
        return await this.FindOwned(id, userId, cancellationToken)
            ?? throw ConversationErrors.NotFound(id);
    }

    public async Task<Conversation?> FindOwned(ConversationId id, Guid userId, CancellationToken cancellationToken)
    {
        Conversation? conversation = await repository.FindById(id, cancellationToken);
        if (conversation is not null && conversation.UserId != userId)
        {
            throw ConversationErrors.NotFound(id);
        }

        return conversation;
    }
}
