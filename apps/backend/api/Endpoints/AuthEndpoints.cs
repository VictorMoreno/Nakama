using Api.Configuration;
using Nakama.Authentication.Application;
using Nakama.Authentication.Application.Authenticator;
using Nakama.Authentication.Application.Closer;
using Nakama.Authentication.Application.Refresher;
using Nakama.Authentication.Domain.Errors;
using Nakama.Shared.Domain;

namespace Api.Endpoints;

public sealed class AuthEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", Login)
            .WithTags("Auth")
            .WithSummary("Verifies email/password credentials, records real activity and issues an access token.")
            .WithDescription(
                "Returns a short-lived JWT access token (to send as 'Authorization: Bearer <token>'), its expiry and the user, and sets the refresh-token cookie. Anonymous: together with refresh and logout it is reachable without a token.")
            .AllowAnonymous()
            .Produces<TokenResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        app.MapPost("auth/refresh", Refresh)
            .WithTags("Auth")
            .WithSummary("Exchanges the refresh-token cookie for a new access token and rotates the refresh token.")
            .WithDescription(
                "Anonymous: the HttpOnly cookie set at login travels by itself. A refresh token that was already rotated revokes every session of that user.")
            .AllowAnonymous()
            .Produces<TokenResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        app.MapPost("auth/logout", Logout)
            .WithTags("Auth")
            .WithSummary("Revokes the refresh token of the current session and clears its cookie.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> Login(LoginRequest body, HttpContext http, IQueryBus queryBus, CancellationToken ct)
    {
        AuthenticationResponse session = await queryBus.Ask(new LogIn.Query(body.Email, body.Password), ct);
        RefreshTokenCookie.Set(http.Response, session.RefreshToken, session.RefreshTokenExpiresAt);
        return Results.Ok(session.Token);
    }

    private static async Task<IResult> Refresh(HttpContext http, IQueryBus queryBus, CancellationToken ct)
    {
        string? refreshToken = RefreshTokenCookie.Read(http.Request);
        if (refreshToken is null)
        {
            throw AuthErrors.InvalidRefreshToken;
        }

        try
        {
            AuthenticationResponse session = await queryBus.Ask(new LogRefresher.Query(refreshToken), ct);
            RefreshTokenCookie.Set(http.Response, session.RefreshToken, session.RefreshTokenExpiresAt);
            return Results.Ok(session.Token);
        }
        catch (DomainError)
        {
            RefreshTokenCookie.Clear(http.Response);
            throw;
        }
    }

    private static async Task<IResult> Logout(HttpContext http, ICommandBus commandBus, CancellationToken ct)
    {
        string? refreshToken = RefreshTokenCookie.Read(http.Request);
        if (refreshToken is not null)
        {
            await commandBus.Send(new LogOut.Command(refreshToken), ct);
        }

        RefreshTokenCookie.Clear(http.Response);
        return Results.NoContent();
    }

    private sealed record LoginRequest(string Email, string Password);
}
