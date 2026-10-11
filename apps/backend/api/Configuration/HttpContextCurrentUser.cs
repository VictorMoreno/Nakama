using Microsoft.IdentityModel.JsonWebTokens;
using Nakama.Shared.Domain;
using Nakama.Users.Domain;

namespace Api.Configuration;

public sealed class HttpContextCurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid Id
    {
        get
        {
            string? subject = this.Principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(subject, out Guid id)
                ? id
                : throw new InvalidOperationException("The authenticated principal has no valid subject claim.");
        }
    }

    public bool IsAdmin => this.Principal.IsInRole(UserRoles.Admin);

    private System.Security.Claims.ClaimsPrincipal Principal =>
        httpContextAccessor.HttpContext?.User
        ?? throw new InvalidOperationException("There is no active HTTP request to read the current user from.");
}
