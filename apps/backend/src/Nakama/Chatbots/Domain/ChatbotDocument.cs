namespace Nakama.Chatbots.Domain;

public sealed record ChatbotDocument(Guid Id, string Name, string Origin, DocStatus Status, string StatusLabel, DateTime Updated, string FileName);
