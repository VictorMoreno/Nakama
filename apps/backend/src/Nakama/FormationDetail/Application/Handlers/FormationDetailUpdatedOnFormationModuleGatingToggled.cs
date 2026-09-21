using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationModuleGatingToggled(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationModuleGatingToggled>
{
    protected override async Task Handle(FormationModuleGatingToggled formationModuleGatingToggled, CancellationToken ct)
    {
        var formationDetail = await formationDetailRepository.FindById(formationModuleGatingToggled.Id.Value, ct);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.ToggleModuleGating(formationModuleGatingToggled.ModuleId.Value, formationModuleGatingToggled.Gated);
        await formationDetailRepository.Update(formationDetail, ct);
    }
}
