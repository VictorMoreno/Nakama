using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.ModuleAdder;

public static class FormationModuleAdder
{
    public sealed record Command(
        Guid Id,
        Guid ModuleId,
        string Title,
        ModuleType Type,
        string Summary,
        bool Gated,
        bool AiDraft,
        string? ResourceUrl,
        int? DurationMin,
        IReadOnlyList<QuizQuestion>? Quiz,
        int? PassingScore) : ICommand;

    public sealed class Handler(
        IFormationsRepository formationsRepository,
        IEventBus eventBus) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            var key = FormationId.From(cmd.Id);
            var formation = await formationsRepository.FindById(key, ct)
                ?? throw FormationErrors.NotFound(cmd.Id.ToString());

            var moduleId = ModuleId.From(cmd.ModuleId.ToString());
            if (formation.Modules.Any(m => m.Id == moduleId))
            {
                throw FormationErrors.ModuleAlreadyExists(cmd.ModuleId.ToString(), cmd.Id.ToString());
            }

            var module = new FormationModule(
                moduleId,
                cmd.Title,
                cmd.Type,
                cmd.Summary,
                cmd.Gated,
                cmd.AiDraft,
                cmd.ResourceUrl,
                cmd.DurationMin,
                cmd.Quiz,
                cmd.PassingScore);

            formation.AddModule(module);

            await formationsRepository.Save(formation, ct);
            await eventBus.Publish(formation, ct);
        }
    }
}
