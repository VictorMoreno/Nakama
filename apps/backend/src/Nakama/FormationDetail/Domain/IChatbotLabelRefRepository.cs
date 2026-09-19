namespace Nakama.FormationDetail.Domain;

public interface IChatbotLabelRefRepository
{
    Task<ChatbotLabelRef?> FindById(Guid id, CancellationToken ct);

    Task Save(ChatbotLabelRef entry, CancellationToken ct);
}
