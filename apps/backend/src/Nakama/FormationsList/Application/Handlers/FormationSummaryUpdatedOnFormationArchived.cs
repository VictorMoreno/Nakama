using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

public sealed class FormationSummaryUpdatedOnFormationArchived(IFormationSummaryRepository formationSummaryRepository)
    : DomainEventHandler<FormationArchived>
{
    protected override async Task Handle(FormationArchived formationArchived, CancellationToken cancellationToken)
    {
        FormationSummary? formationSummary = await formationSummaryRepository.FindById(formationArchived.Id.Value, cancellationToken);
        if (formationSummary is null)
        {
            return;
        }

        formationSummary.MarkArchived();
        await formationSummaryRepository.Update(formationSummary, cancellationToken);
    }
}
