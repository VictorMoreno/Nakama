using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

public sealed class FormationSummaryUpdatedOnFormationModuleCompleted(IFormationSummaryRepository formationSummaryRepository)
    : DomainEventHandler<FormationModuleCompleted>
{
    protected override async Task Handle(FormationModuleCompleted formationModuleCompleted, CancellationToken ct)
    {
        var formationSummary = await formationSummaryRepository.FindById(formationModuleCompleted.Id.Value, ct);
        if (formationSummary is null)
        {
            return;
        }

        formationSummary.IncrementCompletedModulesCount();
        await formationSummaryRepository.Update(formationSummary, ct);
    }
}
