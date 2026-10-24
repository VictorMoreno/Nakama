using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationDocumentAdded(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationDocumentAdded>
{
    protected override async Task Handle(FormationDocumentAdded formationDocumentAdded, CancellationToken cancellationToken)
    {
        Domain.FormationDetail? formationDetail = await formationDetailRepository.FindById(formationDocumentAdded.Id.Value, cancellationToken);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.AddDocument(
            formationDocumentAdded.Document.Id,
            formationDocumentAdded.Document.Name,
            formationDocumentAdded.Document.Origin,
            formationDocumentAdded.Document.Updated,
            formationDocumentAdded.Document.FileName);
        await formationDetailRepository.Update(formationDetail, cancellationToken);
    }
}
