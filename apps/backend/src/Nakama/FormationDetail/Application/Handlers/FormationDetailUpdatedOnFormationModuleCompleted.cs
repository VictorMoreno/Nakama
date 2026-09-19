using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

public sealed class FormationDetailUpdatedOnFormationModuleCompleted(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationModuleCompleted>
{
    protected override async Task Handle(FormationModuleCompleted formationModuleCompleted, CancellationToken ct)
    {
        var formationDetail = await formationDetailRepository.FindById(formationModuleCompleted.Id.Value, ct);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.CompleteModule(formationModuleCompleted.ModuleId.Value, formationModuleCompleted.Score);
        await formationDetailRepository.Update(formationDetail, ct);
    }
}
