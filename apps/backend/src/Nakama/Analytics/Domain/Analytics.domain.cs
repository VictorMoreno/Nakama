namespace Nakama.Analytics.Domain;

public partial class Analytics
{
    public Analytics(
        string id,
        int activeBotsCount,
        int usefulFeedbackCount,
        int totalFeedbackCount)
    {
        this.Id = id;
        this.ActiveBotsCount = activeBotsCount;
        this.UsefulFeedbackCount = usefulFeedbackCount;
        this.TotalFeedbackCount = totalFeedbackCount;
    }

    public static Analytics CreateFromChatbotCreated()
    {
        Analytics analytics = new(SingletonKey, 0, 0, 0);
        analytics.ApplyChatbotCreated();
        return analytics;
    }

    public static Analytics CreateFromFeedbackSubmitted(bool useful)
    {
        Analytics analytics = new(SingletonKey, 0, 0, 0);
        analytics.ApplyFeedbackSubmitted(useful);
        return analytics;
    }

    public void ApplyChatbotCreated() => this.ActiveBotsCount++;

    public void ApplyFeedbackSubmitted(bool useful)
    {
        this.TotalFeedbackCount++;
        if (useful)
        {
            this.UsefulFeedbackCount++;
        }
    }
}
