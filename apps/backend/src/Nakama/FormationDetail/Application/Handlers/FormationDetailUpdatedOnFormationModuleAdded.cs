using MediatR;
using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

/// <summary>Updates <see cref="FormationDetail"/> from <see cref="FormationModuleAdded"/>.</summary>
public sealed class FormationDetailUpdatedOnFormationModuleAdded(IFormationDetailRepository formationDetailRepository)
    : INotificationHandler<DomainEventNotification<FormationModuleAdded>>
{
    public async Task Handle(DomainEventNotification<FormationModuleAdded> n, CancellationToken ct)
    {
        var entry = await formationDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        var quiz = n.DomainEvent.Module.Quiz?.Select(q => new QuizQuestionRow
        {
            Question = q.Question,
            Options = [.. q.Options],
            CorrectIndex = q.CorrectIndex,
        }).ToList();

        entry.AddModule(
            n.DomainEvent.Module.Id.Value,
            n.DomainEvent.Module.Title,
            n.DomainEvent.Module.Type.ToString(),
            n.DomainEvent.Module.Summary,
            n.DomainEvent.Module.Gated,
            n.DomainEvent.Module.AiDraft,
            n.DomainEvent.Module.ResourceUrl,
            n.DomainEvent.Module.DurationMin,
            n.DomainEvent.Module.PassingScore,
            quiz);
        await formationDetailRepository.Save(entry, ct);
    }
}
