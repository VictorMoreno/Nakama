using MediatR;
using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

/// <summary>Updates <see cref="FormationDetail"/> from <see cref="FormationModuleGatingToggled"/>.</summary>
public sealed class FormationDetailUpdatedOnFormationModuleGatingToggled(IFormationDetailRepository formationDetailRepository)
    : INotificationHandler<DomainEventNotification<FormationModuleGatingToggled>>
{
    public async Task Handle(DomainEventNotification<FormationModuleGatingToggled> n, CancellationToken ct)
    {
        var entry = await formationDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.ToggleModuleGating(n.DomainEvent.ModuleId.Value, n.DomainEvent.Gated);
        await formationDetailRepository.Save(entry, ct);
    }
}
