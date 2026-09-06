namespace Nakama.Analytics.Domain;

public sealed class UsageByBotRow
{
    public string BotKey { get; set; } = string.Empty;
    public int Value { get; set; }
}
