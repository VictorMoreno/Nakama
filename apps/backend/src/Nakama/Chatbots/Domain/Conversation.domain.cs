using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain;

public partial class Conversation
{
    public Conversation(
        ConversationId id,
        ChatbotId botKey,
        Guid userId,
        string title,
        DateTime startedAt,
        IReadOnlyList<ConversationExchange> exchanges)
    {
        this.Id = id;
        this.BotKey = botKey;
        this.UserId = userId;
        this.Title = title;
        this.StartedAt = startedAt;
        this._exchanges.AddRange(exchanges);
    }

    public static Conversation Start(ConversationId id, ChatbotId botKey, Guid userId, string title, ISystemClock systemClock)
    {
        DateTime startedAt = systemClock.UtcNow;

        Conversation conversation = new(id, botKey, userId, title, startedAt, []);
        conversation.Record(new ConversationStarted(id, botKey, userId, title, startedAt));

        return conversation;
    }

    public void AppendExchange(ConversationExchange exchange, ISystemClock systemClock)
    {
        this._exchanges.Add(exchange);
        this.Record(new ConversationExchangeAppended(this.Id, this.BotKey, this.UserId, exchange, systemClock.UtcNow));
    }

    public void RateExchange(int index, bool useful, ISystemClock systemClock)
    {
        if (index < 0 || index >= this._exchanges.Count)
        {
            throw ConversationErrors.ExchangeNotFound(this.Id, index);
        }

        ConversationExchange rated = this._exchanges[index] with { Useful = useful };
        this._exchanges[index] = rated;
        this.Record(new ConversationExchangeRated(this.Id, this.BotKey, this.UserId, index, rated.Question, useful, systemClock.UtcNow));
    }

    public void Delete() => this.Record(new ConversationDeleted(
        this.Id,
        this.BotKey,
        this._exchanges.SelectMany(e => e.Attachments ?? []).Select(a => a.FileName).ToList()));
}
