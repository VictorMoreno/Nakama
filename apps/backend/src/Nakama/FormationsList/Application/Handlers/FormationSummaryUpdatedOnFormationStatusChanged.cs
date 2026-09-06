using MediatR;
using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

/// <summary>Updates <see cref="FormationSummary"/> from <see cref="FormationStatusChanged"/>.</summary>
public sealed class FormationSummaryUpdatedOnFormationStatusChanged(IFormationSummaryRepository formationSummaryRepository)
    : INotificationHandler<DomainEventNotification<FormationStatusChanged>>
{
    public async Task Handle(DomainEventNotification<FormationStatusChanged> n, CancellationToken ct)
    {
        var entry = await formationSummaryRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.UpdateStatus(n.DomainEvent.Status.ToString());
        await formationSummaryRepository.Save(entry, ct);
    }
}
