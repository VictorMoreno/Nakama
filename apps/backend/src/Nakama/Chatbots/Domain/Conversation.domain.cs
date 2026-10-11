using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public partial class Conversation
{
    private Conversation(ConversationId id, ChatbotId botKey, Guid userId, string title, DateTime startedAt)
    {
        this.Id = id;
        this.BotKey = botKey;
        this.UserId = userId;
        this.Title = title;
        this.StartedAt = startedAt;
    }

    public static Conversation Start(ConversationId id, ChatbotId botKey, Guid userId, string title, ISystemClock systemClock)
    {
        DateTime startedAt = systemClock.UtcNow;

        Conversation conversation = new(id, botKey, userId, title, startedAt);
        conversation.Record(new ConversationStarted(id, botKey, userId, title, startedAt));

        return conversation;
    }

    public void AppendExchange(ConversationExchange exchange)
    {
        this._exchanges.Add(exchange);
        this.Record(new ConversationExchangeAppended(this.Id, this.BotKey, this.UserId, exchange));
    }

    public void Delete() => this.Record(new ConversationDeleted(
        this.Id,
        this.BotKey,
        this._exchanges.SelectMany(e => e.Attachments ?? []).Select(a => a.FileName).ToList()));
}
