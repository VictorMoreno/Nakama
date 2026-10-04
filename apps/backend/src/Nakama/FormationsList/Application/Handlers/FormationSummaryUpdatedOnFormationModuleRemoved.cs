using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

public sealed class FormationSummaryUpdatedOnFormationModuleRemoved(IFormationSummaryRepository formationSummaryRepository)
    : DomainEventHandler<FormationModuleRemoved>
{
    protected override async Task Handle(FormationModuleRemoved formationModuleRemoved, CancellationToken cancellationToken)
    {
        var formationSummary = await formationSummaryRepository.FindById(formationModuleRemoved.Id.Value, cancellationToken);
        if (formationSummary is null)
        {
            return;
        }

        formationSummary.DecrementModulesCount();
        await formationSummaryRepository.Update(formationSummary, cancellationToken);
    }
}
