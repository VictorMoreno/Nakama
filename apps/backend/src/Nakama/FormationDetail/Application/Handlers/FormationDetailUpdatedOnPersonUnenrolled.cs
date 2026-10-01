using Nakama.FormationDetail.Domain;
using Nakama.FormationEnrollment.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnPersonUnenrolled(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<PersonUnenrolled>
{
    protected override async Task Handle(PersonUnenrolled personUnenrolled, CancellationToken ct)
    {
        var formationDetail = await formationDetailRepository.FindById(personUnenrolled.FormationId, ct);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.RemoveMember(personUnenrolled.UserId);
        await formationDetailRepository.Update(formationDetail, ct);
    }
}
