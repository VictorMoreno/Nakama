using Nakama.Shared.Domain;

namespace Nakama.ChatPage.Domain.Errors;

public sealed class ChatPageNotFoundError(string botKey)
    : DomainError("nakama.chat_page.not_found", $"ChatPage '{botKey}' not found.");

public static class ChatPageErrors
{
    public static DomainError NotFound(string botKey) => new ChatPageNotFoundError(botKey);
}
