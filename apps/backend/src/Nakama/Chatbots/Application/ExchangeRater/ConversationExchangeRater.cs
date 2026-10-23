using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Chatbots.Application.ExchangeRater;

public static class ConversationExchangeRater
{
    public sealed record Command(Guid BotKey, Guid ConversationId, int ExchangeIndex, bool Useful, Guid UserId) : ICommand;

    public sealed class Handler(Rater rater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct) =>
            await rater.Execute(cmd, ct);
    }

    public sealed class Rater(IConversationsRepository repository, IEventBus eventBus, ISystemClock systemClock) : IService
    {
        private readonly ConversationFinderById _finder = new(repository);

        public async Task Execute(Command cmd, CancellationToken cancellationToken)
        {
            ConversationId id = ConversationId.From(cmd.ConversationId);
            Conversation conversation = await this._finder.Execute(id, cmd.UserId, cancellationToken);

            if (conversation.BotKey.Value != cmd.BotKey)
            {
                throw ConversationErrors.NotFound(id);
            }

            conversation.RateExchange(cmd.ExchangeIndex, cmd.Useful, systemClock);

            await repository.Save(conversation, cancellationToken);
            await eventBus.Publish(conversation, cancellationToken);
        }
    }
}
