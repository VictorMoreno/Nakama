using Nakama.ChatPage.Domain;

namespace Nakama.ChatPage.Application;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

public sealed record ChatPageResponse(
    Guid Id,
    string Label,
    string Context,
    bool Socratic,
    bool ExplicaMas,
    string Provider,
    ChatExampleRow? Opening,
    List<ChatExampleRow> Chips,
    List<ConversationSummaryRow> History)
{
    public static explicit operator ChatPageResponse(ChatPage entity) => new(
        entity.Id,
        entity.Label,
        entity.Context,
        entity.Socratic,
        entity.ExplicaMas,
        entity.Provider,
        entity.Opening,
        entity.Chips,
        entity.History);
}
