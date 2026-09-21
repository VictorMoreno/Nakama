using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
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

    public sealed class Handler(Adder adder) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await adder.Execute(FormationId.From(cmd.Id), cmd, ct);
        }
    }

    public sealed class Adder(IFormationsRepository repository, IEventBus eventBus) : IService
    {
        private readonly FormationFinderById _finder = new(repository);

        public async Task Execute(FormationId id, Command cmd, CancellationToken cancellationToken)
        {
            Formation formation = await _finder.Execute(id, cancellationToken);

            var moduleId = ModuleId.From(cmd.ModuleId);
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

            await repository.Save(formation, cancellationToken);
            await eventBus.Publish(formation, cancellationToken);
        }
    }
}