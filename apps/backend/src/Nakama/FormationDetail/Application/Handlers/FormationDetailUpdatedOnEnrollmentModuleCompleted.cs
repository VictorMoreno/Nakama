using Nakama.FormationDetail.Domain;
using Nakama.FormationEnrollment.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnEnrollmentModuleCompleted(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<EnrollmentModuleCompleted>
{
    protected override async Task Handle(EnrollmentModuleCompleted enrollmentModuleCompleted, CancellationToken cancellationToken)
    {
        var formationDetail = await formationDetailRepository.FindById(enrollmentModuleCompleted.FormationId, cancellationToken);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.UpdateMemberProgress(
            enrollmentModuleCompleted.UserId,
            enrollmentModuleCompleted.CompletedModuleCount,
            formationDetail.Modules.Count);
        await formationDetailRepository.Update(formationDetail, cancellationToken);
    }
}
