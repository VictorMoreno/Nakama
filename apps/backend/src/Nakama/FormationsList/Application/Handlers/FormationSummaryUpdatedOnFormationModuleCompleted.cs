using MediatR;
using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

/// <summary>Updates <see cref="FormationSummary"/> from <see cref="FormationModuleCompleted"/>.</summary>
public sealed class FormationSummaryUpdatedOnFormationModuleCompleted(IFormationSummaryRepository formationSummaryRepository)
    : INotificationHandler<DomainEventNotification<FormationModuleCompleted>>
{
    public async Task Handle(DomainEventNotification<FormationModuleCompleted> n, CancellationToken ct)
    {
        var entry = await formationSummaryRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.IncrementCompletedModulesCount();
        await formationSummaryRepository.Save(entry, ct);
    }
}
