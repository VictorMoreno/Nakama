using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public partial class Conversation
{
    private Conversation(ConversationId id, ChatbotId botKey, string title, DateTime startedAt)
    {
        this.Id = id;
        this.BotKey = botKey;
        this.Title = title;
        this.StartedAt = startedAt;
    }

    public static Conversation Start(ConversationId id, ChatbotId botKey, string title, ISystemClock systemClock)
    {
        var conversation = new Conversation(id, botKey, title, systemClock.UtcNow);
        conversation.Record(new ConversationStarted(id, botKey, title));
        return conversation;
    }

    public void AppendExchange(ConversationExchange exchange)
    {
        this._exchanges.Add(exchange);
        Record(new ConversationExchangeAppended(this.Id, this.BotKey, exchange));
    }
}
