namespace Nakama.Chatbots.Domain;

public sealed record ChatbotDocument(
    Guid Id,
    string Name,
    string Origin,
    DateTime Updated,
    string FileName);
