using MediatR;
using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

/// <summary>Updates <see cref="FormationDetail"/> from <see cref="FormationModuleCompleted"/>.</summary>
public sealed class FormationDetailUpdatedOnFormationModuleCompleted(IFormationDetailRepository formationDetailRepository)
    : INotificationHandler<DomainEventNotification<FormationModuleCompleted>>
{
    public async Task Handle(DomainEventNotification<FormationModuleCompleted> n, CancellationToken ct)
    {
        var entry = await formationDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.CompleteModule(n.DomainEvent.ModuleId.Value, n.DomainEvent.Score);
        await formationDetailRepository.Save(entry, ct);
    }
}
