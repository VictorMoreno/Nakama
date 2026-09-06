using MediatR;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.ModuleGatingToggler;

public static class FormationModuleGatingToggler
{
    public sealed record Command(string Key, string ModuleId) : IRequest<Response>;

    public sealed record Response;

    public sealed class Handler(IFormationsRepository formationsRepository, IEventBus eventBus) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command cmd, CancellationToken ct)
        {
            var key = FormationKey.From(cmd.Key);
            var formation = await formationsRepository.FindByKey(key, ct)
                ?? throw FormationErrors.NotFound(cmd.Key);

            formation.ToggleModuleGating(Domain.ModuleId.From(cmd.ModuleId));

            await formationsRepository.Save(formation, ct);
            await eventBus.Publish([formation], ct);

            return new Response();
        }
    }
}
