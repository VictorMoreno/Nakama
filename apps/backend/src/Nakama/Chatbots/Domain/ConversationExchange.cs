namespace Nakama.Chatbots.Domain;

public sealed record ExchangeAttachment(string Name, string FileName);

public sealed record ConversationExchange(
    string Question,
    string Answer,
    IReadOnlyList<ExchangeAttachment>? Attachments = null,
    bool? Useful = null);
