using Nakama.ConversationHistory.Domain;
using Nakama.ConversationHistory.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.ConversationHistory.Application.Finder;

using ConversationHistory = Nakama.ConversationHistory.Domain.ConversationHistory;

public static class ConversationHistoryFinder
{
    public sealed record Query(Guid BotKey, Guid? UserId) : IQuery<ConversationHistoryResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, ConversationHistoryResponse>
    {
        public async Task<ConversationHistoryResponse> Handle(Query query, CancellationToken ct)
        {
            ConversationHistory history = await finder.Execute(query.BotKey, ct);
            ConversationHistoryResponse response = (ConversationHistoryResponse)history;

            return response with { Entries = response.Entries.Where(e => e.UserId == query.UserId).ToList() };
        }
    }

    public sealed class Finder(IConversationHistoryRepository repository) : IService
    {
        public async Task<ConversationHistory> Execute(Guid botKey, CancellationToken cancellationToken) =>
            await repository.FindById(botKey, cancellationToken)
                ?? throw ConversationHistoryErrors.NotFound(botKey.ToString());
    }
}
