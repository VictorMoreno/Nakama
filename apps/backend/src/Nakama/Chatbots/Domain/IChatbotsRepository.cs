namespace Nakama.Chatbots.Domain;

public interface IChatbotsRepository
{
    Task<Chatbot?> FindByKey(ChatbotKey key, CancellationToken ct);

    Task Save(Chatbot chatbot, CancellationToken ct);
}
