using MediatR;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.ExchangeAppender;

public static class ConversationExchangeAppender
{
    /// <summary>
    /// When <paramref name="ConversationId"/> is null, a new <see cref="Conversation"/> is
    /// started for <paramref name="BotKey"/> - this is the write-side entry point for both
    /// "first message in a new chat" and "reply within an existing conversation".
    /// </summary>
    public sealed record Command(
        string? ConversationId,
        string BotKey,
        string Title,
        string Question,
        string Answer,
        string Citation,
        string MoreExplanation,
        string SocraticVariant) : IRequest<Response>;

    public sealed record Response(string ConversationId);

    public sealed class Handler(
        IConversationsRepository conversationsRepository,
        IEventBus eventBus,
        IUuidGenerator uuidGenerator,
        ISystemClock systemClock) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command cmd, CancellationToken ct)
        {
            var botKey = ChatbotKey.From(cmd.BotKey);

            var conversation = cmd.ConversationId is not null
                ? await conversationsRepository.FindByKey(Domain.ConversationId.From(cmd.ConversationId), ct)
                    ?? throw ConversationErrors.NotFound(cmd.ConversationId)
                : Conversation.Start(Domain.ConversationId.From(uuidGenerator.NewId()), botKey, cmd.Title, systemClock);

            conversation.AppendExchange(new ConversationExchange(cmd.Question, cmd.Answer, cmd.Citation, cmd.MoreExplanation, cmd.SocraticVariant));

            await conversationsRepository.Save(conversation, ct);
            await eventBus.Publish([conversation], ct);

            return new Response(conversation.Id.Value);
        }
    }
}
