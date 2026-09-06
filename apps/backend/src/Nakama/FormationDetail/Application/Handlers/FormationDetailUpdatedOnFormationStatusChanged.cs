using MediatR;
using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

/// <summary>Updates <see cref="FormationDetail"/> from <see cref="FormationStatusChanged"/>.</summary>
public sealed class FormationDetailUpdatedOnFormationStatusChanged(IFormationDetailRepository formationDetailRepository)
    : INotificationHandler<DomainEventNotification<FormationStatusChanged>>
{
    public async Task Handle(DomainEventNotification<FormationStatusChanged> n, CancellationToken ct)
    {
        var entry = await formationDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.UpdateStatus(n.DomainEvent.Status.ToString());
        await formationDetailRepository.Save(entry, ct);
    }
}
