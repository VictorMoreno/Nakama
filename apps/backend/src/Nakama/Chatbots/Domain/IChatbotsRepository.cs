namespace Nakama.Chatbots.Domain;

public interface IChatbotsRepository
{
    Task<Chatbot?> FindById(ChatbotId id, CancellationToken ct);

    Task Save(Chatbot chatbot, CancellationToken ct);
}
