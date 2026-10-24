using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationUnarchived(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationUnarchived>
{
    protected override async Task Handle(FormationUnarchived formationUnarchived, CancellationToken cancellationToken)
    {
        Domain.FormationDetail? formationDetail = await formationDetailRepository.FindById(formationUnarchived.Id.Value, cancellationToken);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.MarkUnarchived();
        await formationDetailRepository.Update(formationDetail, cancellationToken);
    }
}
