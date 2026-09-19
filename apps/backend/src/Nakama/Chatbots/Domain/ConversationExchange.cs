namespace Nakama.Chatbots.Domain;

public sealed record ConversationExchange(string Question, string Answer, string Citation, string MoreExplanation, string SocraticVariant);
