using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

public sealed class FormationSummaryUpdatedOnFormationModuleAdded(IFormationSummaryRepository formationSummaryRepository)
    : DomainEventHandler<FormationModuleAdded>
{
    protected override async Task Handle(FormationModuleAdded formationModuleAdded, CancellationToken cancellationToken)
    {
        FormationSummary? formationSummary = await formationSummaryRepository.FindById(formationModuleAdded.Id.Value, cancellationToken);
        if (formationSummary is null)
        {
            return;
        }

        formationSummary.IncrementModulesCount();
        await formationSummaryRepository.Update(formationSummary, cancellationToken);
    }
}
