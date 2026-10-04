using Nakama.FormationDetail.Domain;
using Nakama.FormationEnrollment.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnPersonEnrolled(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<PersonEnrolled>
{
    protected override async Task Handle(PersonEnrolled personEnrolled, CancellationToken cancellationToken)
    {
        var formationDetail = await formationDetailRepository.FindById(personEnrolled.FormationId, cancellationToken);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.AddMember(personEnrolled.UserId);
        await formationDetailRepository.Update(formationDetail, cancellationToken);
    }
}
