namespace Nakama.Chatbots.Domain;

/// <summary>Mirrors the frontend's <c>ChatExample</c> shape (question/answer/citation/deeper-explanation/socratic variant).</summary>
public sealed record ConversationExchange(string Question, string Answer, string Citation, string MoreExplanation, string SocraticVariant);
