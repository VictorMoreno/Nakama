using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationModuleRemoved(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationModuleRemoved>
{
    protected override async Task Handle(FormationModuleRemoved formationModuleRemoved, CancellationToken ct)
    {
        var formationDetail = await formationDetailRepository.FindById(formationModuleRemoved.Id.Value, ct);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.RemoveModule(formationModuleRemoved.ModuleId.Value);
        await formationDetailRepository.Update(formationDetail, ct);
    }
}
