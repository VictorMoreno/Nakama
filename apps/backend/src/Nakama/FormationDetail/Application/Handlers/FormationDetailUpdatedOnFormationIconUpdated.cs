using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationIconUpdated(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationIconUpdated>
{
    protected override async Task Handle(FormationIconUpdated formationIconUpdated, CancellationToken ct)
    {
        Domain.FormationDetail? formationDetail =
            await formationDetailRepository.FindById(formationIconUpdated.Id.Value, ct);

        if (formationDetail is null)
        {
            return;
        }

        formationDetail.UpdateIcon(formationIconUpdated.IconUrl);
        await formationDetailRepository.Update(formationDetail, ct);
    }
}