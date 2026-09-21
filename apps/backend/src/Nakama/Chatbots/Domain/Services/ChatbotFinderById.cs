using Nakama.Chatbots.Domain.Errors;

namespace Nakama.Chatbots.Domain.Services;

public sealed class ChatbotFinderById(IChatbotsRepository repository)
{
    public async Task<Chatbot> Execute(ChatbotId id, CancellationToken cancellationToken)
    {
        return await repository.FindById(id, cancellationToken)
            ?? throw ChatbotErrors.NotFound(id);
    }
}