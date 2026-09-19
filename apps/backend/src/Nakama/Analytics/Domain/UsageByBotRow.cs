namespace Nakama.Analytics.Domain;

public sealed class UsageByBotRow
{
    public Guid BotKey { get; set; }
    public int Value { get; set; }
}
