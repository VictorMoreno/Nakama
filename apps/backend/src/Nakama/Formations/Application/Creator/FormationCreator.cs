using MediatR;
using Nakama.Formations.Domain;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.Formations.Application.Creator;

public static class FormationCreator
{
    public sealed record Command(
        string Title,
        string Audience,
        string Description,
        FormationStatus Status,
        string? LinkedChatbotKey) : IRequest<Response>;

    public sealed record Response(string Key);

    public sealed class Handler(
        IFormationsRepository formationsRepository,
        IEventBus eventBus,
        IUuidGenerator uuidGenerator) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command cmd, CancellationToken ct)
        {
            var formation = Formation.Create(
                FormationKey.From(uuidGenerator.NewId()),
                cmd.Title,
                cmd.Audience,
                cmd.Description,
                cmd.Status,
                cmd.LinkedChatbotKey);

            await formationsRepository.Save(formation, ct);
            await eventBus.Publish([formation], ct);

            return new Response(formation.Key.Value);
        }
    }
}
