using MediatR;
using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

/// <summary>
/// Creates <see cref="FormationDetail"/>, the read entity behind the <c>/formacion/:key</c>
/// route. Resolves the denormalized <c>LinkedChatbotLabel</c> caption via the local
/// <see cref="ChatbotLabelRef"/> lookup, kept current by <see cref="ChatbotLabelRefCreatedOnChatbotCreated"/>.
/// </summary>
public sealed class FormationDetailCreatedOnFormationCreated(
    IFormationDetailRepository formationDetailRepository,
    IChatbotLabelRefRepository chatbotLabelRefRepository) : INotificationHandler<DomainEventNotification<FormationCreated>>
{
    public async Task Handle(DomainEventNotification<FormationCreated> n, CancellationToken ct)
    {
        string? linkedChatbotLabel = null;
        if (n.DomainEvent.LinkedChatbotKey is not null)
        {
            var labelRef = await chatbotLabelRefRepository.FindByBotKey(n.DomainEvent.LinkedChatbotKey, ct);
            linkedChatbotLabel = labelRef?.Label;
        }

        await formationDetailRepository.Save(
            FormationDetail.Create(
                n.DomainEvent.Key.Value,
                n.DomainEvent.Title,
                n.DomainEvent.Audience,
                n.DomainEvent.Description,
                n.DomainEvent.Status.ToString(),
                n.DomainEvent.LinkedChatbotKey,
                linkedChatbotLabel),
            ct);
    }
}
