namespace Nakama.Analytics.Domain;

public partial class Analytics
{
    public Analytics(
        string id,
        int activeBotsCount)
    {
        this.Id = id;
        this.ActiveBotsCount = activeBotsCount;
    }

    public static Analytics CreateFromChatbotCreated()
    {
        Analytics analytics = new(SingletonKey, 0);
        analytics.ApplyChatbotCreated();
        return analytics;
    }

    public void ApplyChatbotCreated() => this.ActiveBotsCount++;
}
