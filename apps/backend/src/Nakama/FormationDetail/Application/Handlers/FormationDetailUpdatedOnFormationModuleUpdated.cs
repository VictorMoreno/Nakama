using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationModuleUpdated(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationModuleUpdated>
{
    protected override async Task Handle(FormationModuleUpdated formationModuleUpdated, CancellationToken cancellationToken)
    {
        var formationDetail = await formationDetailRepository.FindById(formationModuleUpdated.Id.Value, cancellationToken);
        if (formationDetail is null)
        {
            return;
        }

        var quiz = formationModuleUpdated.Module.Quiz?.Select(q => new QuizQuestionRow
        {
            Question = q.Question,
            Options = [.. q.Options],
            CorrectIndex = q.CorrectIndex,
        }).ToList();

        formationDetail.UpdateModule(
            formationModuleUpdated.Module.Id.Value,
            formationModuleUpdated.Module.Title,
            formationModuleUpdated.Module.Type.ToString(),
            formationModuleUpdated.Module.Summary,
            formationModuleUpdated.Module.Gated,
            formationModuleUpdated.Module.AiDraft,
            formationModuleUpdated.Module.ResourceUrl,
            formationModuleUpdated.Module.DurationMin,
            formationModuleUpdated.Module.PassingScore,
            quiz);
        await formationDetailRepository.Update(formationDetail, cancellationToken);
    }
}
