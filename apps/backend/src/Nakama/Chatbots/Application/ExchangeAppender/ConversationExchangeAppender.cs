using Nakama.Chatbots.Domain;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.ExchangeAppender;

public static class ConversationExchangeAppender
{
    public sealed record Command(
        Guid ConversationId,
        Guid BotKey,
        string Title,
        string Question,
        string Answer,
        string Citation,
        string MoreExplanation,
        string SocraticVariant) : ICommand;

    public sealed class Handler(
        IConversationsRepository conversationsRepository,
        IEventBus eventBus,
        ISystemClock systemClock) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            var botKey = ChatbotId.From(cmd.BotKey);
            var conversationId = ConversationId.From(cmd.ConversationId);

            var conversation = await conversationsRepository.FindById(conversationId, ct)
                               ?? Conversation.Start(conversationId, botKey, cmd.Title, systemClock);

            conversation.AppendExchange(new ConversationExchange(cmd.Question, cmd.Answer, cmd.Citation,
                cmd.MoreExplanation, cmd.SocraticVariant));

            await conversationsRepository.Save(conversation, ct);
            await eventBus.Publish(conversation, ct);
        }
    }
}