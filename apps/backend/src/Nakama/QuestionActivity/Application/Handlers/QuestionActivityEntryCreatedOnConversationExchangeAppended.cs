using Nakama.Chatbots.Domain.Events;
using Nakama.QuestionActivity.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.QuestionActivity.Application.Handlers;

public sealed class QuestionActivityEntryCreatedOnConversationExchangeAppended(IQuestionActivityRepository repository)
    : DomainEventHandler<ConversationExchangeAppended>
{
    protected override async Task Handle(ConversationExchangeAppended exchangeAppended, CancellationToken cancellationToken) =>
        await repository.Save(
            QuestionActivityEntry.Create(
                Guid.NewGuid(),
                exchangeAppended.BotKey.Value,
                exchangeAppended.UserId,
                exchangeAppended.OccurredAt),
            cancellationToken);
}
