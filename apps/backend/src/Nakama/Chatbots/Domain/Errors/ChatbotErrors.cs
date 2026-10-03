using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Domain.Errors;

public sealed class ChatbotOwnerCannotBeRemovedError()
    : DomainError("nakama.chatbots.owner_cannot_be_removed", "El rol owner no se puede eliminar del equipo.");

public sealed class ChatbotNotFoundError(string key)
    : DomainError("nakama.chatbots.not_found", $"Chatbot '{key}' not found.");

public sealed class ChatbotAlreadyExistsError(string id)
    : DomainError("nakama.chatbots.already_exists", $"Chatbot '{id}' already exists.");

public sealed class ChatbotProviderNotConnectedError(string provider)
    : DomainError("nakama.chatbots.provider_not_connected", $"El proveedor '{provider}' todavía no tiene un adaptador conectado.");

public static class ChatbotErrors
{
    public static DomainError OwnerCannotBeRemoved { get; } = new ChatbotOwnerCannotBeRemovedError();

    public static DomainError NotFound(ChatbotId id) => new ChatbotNotFoundError(id.ToString());

    public static DomainError AlreadyExists(string id) => new ChatbotAlreadyExistsError(id);

    public static DomainError ProviderNotConnected(string provider) => new ChatbotProviderNotConnectedError(provider);
}
