using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationArchived(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationArchived>
{
    protected override async Task Handle(FormationArchived formationArchived, CancellationToken cancellationToken)
    {
        Domain.FormationDetail? formationDetail = await formationDetailRepository.FindById(formationArchived.Id.Value, cancellationToken);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.MarkArchived();
        await formationDetailRepository.Update(formationDetail, cancellationToken);
    }
}
