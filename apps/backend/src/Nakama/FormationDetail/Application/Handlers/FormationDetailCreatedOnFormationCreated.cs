using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

public sealed class FormationDetailCreatedOnFormationCreated(
    IFormationDetailRepository formationDetailRepository,
    IChatbotLabelRefRepository chatbotLabelRefRepository) : DomainEventHandler<FormationCreated>
{
    protected override async Task Handle(FormationCreated formationCreated, CancellationToken cancellationToken)
    {
        string? linkedChatbotLabel = null;
        if (formationCreated.LinkedChatbotKey is not null)
        {
            ChatbotLabelRef? labelRef = await chatbotLabelRefRepository.FindById(formationCreated.LinkedChatbotKey.Value, cancellationToken);
            linkedChatbotLabel = labelRef?.Label;
        }

        await formationDetailRepository.Save(
            FormationDetail.Create(
                formationCreated.Id.Value,
                formationCreated.Title,
                formationCreated.Audience,
                formationCreated.Description,
                formationCreated.Status.ToString(),
                formationCreated.LinkedChatbotKey,
                linkedChatbotLabel),
            cancellationToken);
    }
}
