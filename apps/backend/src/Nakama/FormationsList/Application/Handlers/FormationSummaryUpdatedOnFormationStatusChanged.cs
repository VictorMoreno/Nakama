using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

public sealed class FormationSummaryUpdatedOnFormationStatusChanged(IFormationSummaryRepository formationSummaryRepository)
    : DomainEventHandler<FormationStatusChanged>
{
    protected override async Task Handle(FormationStatusChanged formationStatusChanged, CancellationToken cancellationToken)
    {
        FormationSummary? formationSummary = await formationSummaryRepository.FindById(formationStatusChanged.Id.Value, cancellationToken);
        if (formationSummary is null)
        {
            return;
        }

        formationSummary.UpdateStatus(formationStatusChanged.Status.ToString());
        await formationSummaryRepository.Update(formationSummary, cancellationToken);
    }
}
