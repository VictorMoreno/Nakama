using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationDetailsUpdated(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationDetailsUpdated>
{
    protected override async Task Handle(FormationDetailsUpdated formationDetailsUpdated, CancellationToken cancellationToken)
    {
        Domain.FormationDetail? formationDetail = await formationDetailRepository.FindById(formationDetailsUpdated.Id.Value, cancellationToken);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.UpdateDetails(formationDetailsUpdated.Title, formationDetailsUpdated.Audience, formationDetailsUpdated.Description);
        await formationDetailRepository.Update(formationDetail, cancellationToken);
    }
}
