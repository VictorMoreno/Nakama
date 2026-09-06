namespace Nakama.Analytics.Domain;

public partial class Analytics
{
    public static Analytics CreateFromChatbotCreated(string botKey)
    {
        var analytics = new Analytics();
        analytics.ApplyChatbotCreated(botKey);
        return analytics;
    }

    public static Analytics CreateFromConversationExchangeAppended(string botKey)
    {
        var analytics = new Analytics();
        analytics.ApplyConversationExchangeAppended(botKey);
        return analytics;
    }

    public void ApplyChatbotCreated(string botKey)
    {
        this.ActiveBotsCount++;
        if (this.UsageByBot.All(u => u.BotKey != botKey))
        {
            this.UsageByBot.Add(new UsageByBotRow { BotKey = botKey, Value = 0 });
        }
    }

    public void ApplyConversationExchangeAppended(string botKey)
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
}
