using Nakama.AnswerFeedback.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.AnswerFeedback.Application.Handlers;

public sealed class AnswerFeedbackSavedOnConversationExchangeRated(IAnswerFeedbackRepository repository)
    : DomainEventHandler<ConversationExchangeRated>
{
    protected override async Task Handle(ConversationExchangeRated exchangeRated, CancellationToken cancellationToken)
    {
        Guid id = AnswerFeedbackEntry.IdOf(exchangeRated.Id.Value, exchangeRated.ExchangeIndex);
        AnswerFeedbackEntry? existing = await repository.FindById(id, cancellationToken);

        if (existing is null)
        {
            await repository.Save(
                AnswerFeedbackEntry.Create(
                    exchangeRated.Id.Value,
                    exchangeRated.ExchangeIndex,
                    exchangeRated.BotKey.Value,
                    exchangeRated.Question,
                    exchangeRated.Useful,
                    exchangeRated.OccurredAt),
                cancellationToken);
            return;
        }

        existing.Rate(exchangeRated.Useful, exchangeRated.OccurredAt);
        await repository.Save(existing, cancellationToken);
    }
}
