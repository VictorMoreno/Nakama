using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationStatusChanged(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationStatusChanged>
{
    protected override async Task Handle(FormationStatusChanged formationStatusChanged, CancellationToken cancellationToken)
    {
        Domain.FormationDetail? formationDetail = await formationDetailRepository.FindById(formationStatusChanged.Id.Value, cancellationToken);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.UpdateStatus(formationStatusChanged.Status.ToString());
        await formationDetailRepository.Update(formationDetail, cancellationToken);
    }
}
