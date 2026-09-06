namespace Nakama.FormationDetail.Domain;

/// <summary>Backing store for <see cref="ChatbotLabelRef"/> - see that type for why it exists.</summary>
public interface IChatbotLabelRefRepository
{
    Task<ChatbotLabelRef?> FindByBotKey(string botKey, CancellationToken ct);

    Task Save(ChatbotLabelRef entry, CancellationToken ct);
}
