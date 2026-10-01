using Nakama.Chatbots.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.ExchangeAppender;

public static class ConversationExchangeAppender
{
    public sealed record Command(
        Guid ConversationId,
        Guid BotKey,
        Guid UserId,
        string Title,
        string Question,
        string Answer,
        string Citation,
        string MoreExplanation,
        string SocraticVariant) : ICommand;

    public sealed class Handler(Appender appender) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await appender.Execute(cmd, ct);
        }
    }

    public sealed class Appender(
        IConversationsRepository repository,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        // No finder here either: this is find-or-implicitly-start (upsert), never throws
        // NotFound - the whole point is that a missing conversation isn't an error.
        public async Task Execute(Command cmd, CancellationToken cancellationToken)
        {
            var botKey = ChatbotId.From(cmd.BotKey);
            var conversationId = ConversationId.From(cmd.ConversationId);

            var conversation = await repository.FindById(conversationId, cancellationToken)
                               ?? Conversation.Start(conversationId, botKey, cmd.UserId, cmd.Title, systemClock);

            conversation.AppendExchange(new ConversationExchange(cmd.Question, cmd.Answer, cmd.Citation,
                cmd.MoreExplanation, cmd.SocraticVariant));

            await repository.Save(conversation, cancellationToken);
            await eventBus.Publish(conversation, cancellationToken);
        }
    }
}