using Nakama.Shared.Domain;

namespace Nakama.ChatbotDetail.Domain.Errors;

public sealed class ChatbotDetailNotFoundError(string key)
    : DomainError("nakama.chatbot_detail.not_found", $"Chatbot '{key}' not found.");

public static class ChatbotDetailErrors
{
    public static DomainError NotFound(string key) => new ChatbotDetailNotFoundError(key);
}
