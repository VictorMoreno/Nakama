namespace Nakama.Analytics.Domain;

public partial class Analytics
{
    public static Analytics CreateFromChatbotCreated(Guid botKey)
    {
        var analytics = new Analytics();
        analytics.ApplyChatbotCreated(botKey);
        return analytics;
    }

    public static Analytics CreateFromConversationExchangeAppended(Guid botKey)
    {
        var analytics = new Analytics();
        analytics.ApplyConversationExchangeAppended(botKey);
        return analytics;
    }

    public static Analytics CreateFromFeedbackSubmitted(bool useful)
    {
        var analytics = new Analytics();
        analytics.ApplyFeedbackSubmitted(useful);
        return analytics;
    }

    public void ApplyChatbotCreated(Guid botKey)
    {
        this.ActiveBotsCount++;
        if (this.UsageByBot.All(u => u.BotKey != botKey))
        {
            this.UsageByBot.Add(new UsageByBotRow { BotKey = botKey, Value = 0 });
        }
    }

    public void ApplyConversationExchangeAppended(Guid botKey)
    {
        this.AnsweredThisWeek++;

        var usage = this.UsageByBot.FirstOrDefault(u => u.BotKey == botKey);

        if (usage is not null)
        {
            usage.Value++;
        }
        else
        {
            this.UsageByBot.Add(new UsageByBotRow { BotKey = botKey, Value = 1 });
        }
    }

    public void ApplyFeedbackSubmitted(bool useful)
    {
        this.TotalFeedbackCount++;
        if (useful)
        {
            this.UsefulFeedbackCount++;
        }
    }
}
