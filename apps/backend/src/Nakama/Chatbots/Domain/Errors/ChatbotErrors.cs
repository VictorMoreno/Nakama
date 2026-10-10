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

public sealed class ChatbotDocumentUnsupportedTypeError()
    : DomainError("nakama.chatbots.document_unsupported_type", "Unsupported document type. Only PDF, Markdown, and plain text files are allowed.");

public sealed class ChatbotDocumentTooLargeError(long maxBytes)
    : DomainError("nakama.chatbots.document_too_large", $"File exceeds the {maxBytes / (1024 * 1024)} MB limit.");

public sealed class ChatbotIconUnsupportedTypeError()
    : DomainError("nakama.chatbots.icon_unsupported_type", "Unsupported image type. Only PNG and JPEG are allowed.");

public sealed class ChatbotIconTooLargeError(long maxBytes)
    : DomainError("nakama.chatbots.icon_too_large", $"File exceeds the {maxBytes / (1024 * 1024)} MB limit.");

public sealed class ChatbotAttachmentLimitExceededError(int maxAttachments)
    : DomainError("nakama.chatbots.attachment_limit_exceeded", $"A message can include at most {maxAttachments} attachments.");

public static class ChatbotErrors
{
    public static DomainError OwnerCannotBeRemoved { get; } = new ChatbotOwnerCannotBeRemovedError();

    public static DomainError DocumentUnsupportedType { get; } = new ChatbotDocumentUnsupportedTypeError();

    public static DomainError IconUnsupportedType { get; } = new ChatbotIconUnsupportedTypeError();

    public static DomainError NotFound(ChatbotId id) => new ChatbotNotFoundError(id.ToString());

    public static DomainError AlreadyExists(string id) => new ChatbotAlreadyExistsError(id);

    public static DomainError ProviderNotConnected(string provider) => new ChatbotProviderNotConnectedError(provider);

    public static DomainError DocumentTooLarge(long maxBytes) => new ChatbotDocumentTooLargeError(maxBytes);

    public static DomainError IconTooLarge(long maxBytes) => new ChatbotIconTooLargeError(maxBytes);

    public static DomainError AttachmentLimitExceeded(int maxAttachments) => new ChatbotAttachmentLimitExceededError(maxAttachments);
}
