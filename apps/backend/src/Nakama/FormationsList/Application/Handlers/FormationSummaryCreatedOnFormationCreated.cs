using MediatR;
using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

/// <summary>Creates <see cref="FormationSummary"/>, the read entity behind the <c>/formacion</c> listing route.</summary>
public sealed class FormationSummaryCreatedOnFormationCreated(IFormationSummaryRepository formationSummaryRepository)
    : INotificationHandler<DomainEventNotification<FormationCreated>>
{
    public Task Handle(DomainEventNotification<FormationCreated> n, CancellationToken ct) =>
        formationSummaryRepository.Save(
            FormationSummary.Create(
                n.DomainEvent.Key.Value,
                n.DomainEvent.Title,
                n.DomainEvent.Audience,
                n.DomainEvent.Description,
                n.DomainEvent.Status.ToString()),
            ct);
}
