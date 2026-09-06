using MediatR;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.ModuleAdder;

public static class FormationModuleAdder
{
    public sealed record Command(
        string Key,
        string Title,
        ModuleType Type,
        string Summary,
        bool Gated,
        bool AiDraft,
        string? ResourceUrl,
        int? DurationMin,
        IReadOnlyList<QuizQuestion>? Quiz,
        int? PassingScore) : IRequest<Response>;

    public sealed record Response(string ModuleId);

    public sealed class Handler(
        IFormationsRepository formationsRepository,
        IEventBus eventBus,
        IUuidGenerator uuidGenerator) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command cmd, CancellationToken ct)
        {
            var key = FormationKey.From(cmd.Key);
            var formation = await formationsRepository.FindByKey(key, ct)
                ?? throw FormationErrors.NotFound(cmd.Key);

            var moduleId = ModuleId.From(uuidGenerator.NewId());
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
            await eventBus.Publish([formation], ct);

            return new Response(moduleId.Value);
        }
    }
}
