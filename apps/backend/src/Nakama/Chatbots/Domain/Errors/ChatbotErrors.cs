using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Errors;

public sealed class ChatbotOwnerCannotBeRemovedError()
    : DomainError("nakama.chatbots.owner_cannot_be_removed", "El rol owner no se puede eliminar del equipo.");

public sealed class ChatbotNotFoundError(string key)
    : DomainError("nakama.chatbots.not_found", $"Chatbot '{key}' not found.");

public static class ChatbotErrors
{
    public static DomainError OwnerCannotBeRemoved { get; } = new ChatbotOwnerCannotBeRemovedError();

    public static DomainError NotFound(string key) => new ChatbotNotFoundError(key);
}
