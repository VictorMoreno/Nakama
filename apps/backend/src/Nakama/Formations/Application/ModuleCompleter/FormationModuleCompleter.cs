using MediatR;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.ModuleCompleter;

public static class FormationModuleCompleter
{
    public sealed record Command(string Key, string ModuleId, int? Score) : IRequest<Response>;

    public sealed record Response;

    /// <summary>
    /// Throws <see cref="Nakama.Formations.Domain.Errors.FormationErrors.ModuleLockedByGating"/>
    /// when the previous module is gated and not yet completed.
    /// </summary>
    public sealed class Handler(IFormationsRepository formationsRepository, IEventBus eventBus) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command cmd, CancellationToken ct)
        {
            var key = FormationKey.From(cmd.Key);
            var formation = await formationsRepository.FindByKey(key, ct)
                ?? throw FormationErrors.NotFound(cmd.Key);

            formation.CompleteModule(Domain.ModuleId.From(cmd.ModuleId), cmd.Score);

            await formationsRepository.Save(formation, ct);
            await eventBus.Publish([formation], ct);

            return new Response();
        }
    }
}
