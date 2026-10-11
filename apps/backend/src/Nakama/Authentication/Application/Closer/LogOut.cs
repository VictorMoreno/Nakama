using Nakama.Authentication.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.Authentication.Application.Closer;

public static class LogOut
{
    public sealed record Command(string RefreshToken) : ICommand;

    public sealed class Handler(Closer closer) : ICommandHandler<Command>
    {
        public Task Handle(Command command, CancellationToken ct) => closer.Execute(command.RefreshToken, ct);
    }

    public sealed class Closer(IRefreshTokensRepository refreshTokens, ISystemClock systemClock) : IService
    {
        public async Task Execute(string refreshToken, CancellationToken ct)
        {
            RefreshToken? token = await refreshTokens.FindByValue(refreshToken, ct);

            if (token is null)
            {
                return;
            }

            token.Revoke(systemClock.UtcNow);

            await refreshTokens.Save(token, ct);
        }
    }
}
