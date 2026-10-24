using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

public sealed class FormationSummaryUpdatedOnFormationUnarchived(IFormationSummaryRepository formationSummaryRepository)
    : DomainEventHandler<FormationUnarchived>
{
    protected override async Task Handle(FormationUnarchived formationUnarchived, CancellationToken cancellationToken)
    {
        FormationSummary? formationSummary = await formationSummaryRepository.FindById(formationUnarchived.Id.Value, cancellationToken);
        if (formationSummary is null)
        {
            return;
        }

        formationSummary.MarkUnarchived();
        await formationSummaryRepository.Update(formationSummary, cancellationToken);
    }
}
