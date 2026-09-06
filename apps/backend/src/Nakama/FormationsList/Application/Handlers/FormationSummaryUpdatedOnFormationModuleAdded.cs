using MediatR;
using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

/// <summary>Updates <see cref="FormationSummary"/> from <see cref="FormationModuleAdded"/>.</summary>
public sealed class FormationSummaryUpdatedOnFormationModuleAdded(IFormationSummaryRepository formationSummaryRepository)
    : INotificationHandler<DomainEventNotification<FormationModuleAdded>>
{
    public async Task Handle(DomainEventNotification<FormationModuleAdded> n, CancellationToken ct)
    {
        var entry = await formationSummaryRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.IncrementModulesCount();
        await formationSummaryRepository.Save(entry, ct);
    }
}
