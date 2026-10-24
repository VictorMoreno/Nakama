using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationDocumentRemoved(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationDocumentRemoved>
{
    protected override async Task Handle(FormationDocumentRemoved formationDocumentRemoved, CancellationToken cancellationToken)
    {
        Domain.FormationDetail? formationDetail = await formationDetailRepository.FindById(formationDocumentRemoved.Id.Value, cancellationToken);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.RemoveDocument(formationDocumentRemoved.DocumentId);
        await formationDetailRepository.Update(formationDetail, cancellationToken);
    }
}
