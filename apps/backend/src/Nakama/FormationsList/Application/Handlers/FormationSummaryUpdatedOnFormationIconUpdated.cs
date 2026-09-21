using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

public sealed class FormationSummaryUpdatedOnFormationIconUpdated(IFormationSummaryRepository formationSummaryRepository)
    : DomainEventHandler<FormationIconUpdated>
{
    protected override async Task Handle(FormationIconUpdated formationIconUpdated, CancellationToken ct)
    {
        var formationSummary = await formationSummaryRepository.FindById(formationIconUpdated.Id.Value, ct);
        if (formationSummary is null)
        {
            return;
        }

        formationSummary.UpdateIcon(formationIconUpdated.IconUrl);
        await formationSummaryRepository.Update(formationSummary, ct);
    }
}
