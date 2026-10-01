using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.ModuleUpdater;

public static class FormationModuleUpdater
{
    public sealed record QuizQuestionInput(string Question, IReadOnlyList<string> Options, int CorrectIndex);

    public sealed record Command(
        Guid Id,
        Guid ModuleId,
        string Title,
        int Type,
        string Summary,
        bool Gated,
        bool AiDraft,
        string? ResourceUrl,
        int? DurationMin,
        IReadOnlyList<QuizQuestionInput>? Quiz,
        int? PassingScore) : ICommand;

    public sealed class Handler(Updater updater) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await updater.Execute(
                FormationId.From(cmd.Id),
                ModuleId.From(cmd.ModuleId),
                cmd.Title,
                (ModuleType)cmd.Type,
                cmd.Summary,
                cmd.Gated,
                cmd.AiDraft,
                cmd.ResourceUrl,
                cmd.DurationMin,
                cmd.Quiz?.Select(q => new QuizQuestion(q.Question, q.Options, q.CorrectIndex)).ToList(),
                cmd.PassingScore,
                ct);
        }
    }

    public sealed class Updater(IFormationsRepository repository, IEventBus eventBus) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(
            FormationId id,
            ModuleId moduleId,
            string title,
            ModuleType type,
            string summary,
            bool gated,
            bool aiDraft,
            string? resourceUrl,
            int? durationMin,
            IReadOnlyList<QuizQuestion>? quiz,
            int? passingScore,
            CancellationToken cancellationToken)
        {
            Formation formation = await _finder.Execute(id, cancellationToken);

            if (formation.Modules.All(m => m.Id != moduleId))
            {
                throw FormationErrors.ModuleNotFound(moduleId.ToString(), id.ToString());
            }

            formation.UpdateModule(
                moduleId, title, type, summary, gated, aiDraft, resourceUrl, durationMin, quiz, passingScore);

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}
