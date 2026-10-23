namespace Nakama.Chatbots.Domain;

public sealed record ExchangeAttachment(string Name, string FileName);

public sealed record ConversationExchange(
    string Question,
    string Answer,
    string Citation,
    string MoreExplanation,
    string SocraticVariant,
    IReadOnlyList<ExchangeAttachment>? Attachments = null,
    bool? Useful = null);
