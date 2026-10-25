using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

public sealed class FormationSummaryUpdatedOnFormationDetailsUpdated(IFormationSummaryRepository formationSummaryRepository)
    : DomainEventHandler<FormationDetailsUpdated>
{
    protected override async Task Handle(FormationDetailsUpdated formationDetailsUpdated, CancellationToken cancellationToken)
    {
        FormationSummary? formationSummary = await formationSummaryRepository.FindById(formationDetailsUpdated.Id.Value, cancellationToken);
        if (formationSummary is null)
        {
            return;
        }

        formationSummary.UpdateDetails(formationDetailsUpdated.Title, formationDetailsUpdated.Audience, formationDetailsUpdated.Description);
        await formationSummaryRepository.Update(formationSummary, cancellationToken);
    }
}
