namespace Nakama.Chatbots.Domain;

public sealed record ChatbotDocument(string Name, string Origin, DocStatus Status, string StatusLabel, DateTime Updated);
