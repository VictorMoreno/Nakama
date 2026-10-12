namespace Nakama.Analytics.Domain;

public partial class Analytics
{
    public Analytics(
        string id,
        int activeBotsCount,
        int answeredThisWeek,
        List<UsageByBotRow> usageByBot,
        int usefulFeedbackCount,
        int totalFeedbackCount)
    {
        this.Id = id;
        this.ActiveBotsCount = activeBotsCount;
        this.AnsweredThisWeek = answeredThisWeek;
        this.UsageByBot = usageByBot;
        this.UsefulFeedbackCount = usefulFeedbackCount;
        this.TotalFeedbackCount = totalFeedbackCount;
    }

    public static Analytics CreateFromChatbotCreated(Guid botKey)
    {
        Analytics analytics = new(SingletonKey, 0, 0, [], 0, 0);
        analytics.ApplyChatbotCreated(botKey);
        return analytics;
    }

    public static Analytics CreateFromConversationExchangeAppended(Guid botKey)
    {
        Analytics analytics = new(SingletonKey, 0, 0, [], 0, 0);
        analytics.ApplyConversationExchangeAppended(botKey);
        return analytics;
    }

    public static Analytics CreateFromFeedbackSubmitted(bool useful)
    {
        Analytics analytics = new(SingletonKey, 0, 0, [], 0, 0);
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

        UsageByBotRow? usage = this.UsageByBot.FirstOrDefault(u => u.BotKey == botKey);

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
