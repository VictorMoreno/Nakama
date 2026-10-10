using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Auth;
using Nakama.Users.Domain;

namespace Nakama.Users.Infrastructure;

public sealed class JwtTokenIssuer : ITokenIssuer
{
    private readonly JwtOptions options;
    private readonly ISystemClock systemClock;
    private readonly SigningCredentials signingCredentials;
    private readonly JsonWebTokenHandler tokenHandler = new();

    public JwtTokenIssuer(IOptions<JwtOptions> options, ISystemClock systemClock)
    {
        this.options = options.Value;
        this.systemClock = systemClock;
        this.signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public AccessToken IssueAccessToken(User user)
    {
        DateTime issuedAt = this.systemClock.UtcNow;
        DateTime expiresAt = issuedAt.AddMinutes(this.options.AccessTokenMinutes);

        SecurityTokenDescriptor descriptor = new()
        {
            Issuer = this.options.Issuer,
            Audience = this.options.Audience,
            IssuedAt = issuedAt,
            NotBefore = issuedAt,
            Expires = expiresAt,
            SigningCredentials = this.signingCredentials,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.Value.ToString(),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                [JwtRegisteredClaimNames.Email] = user.Email,
                [JwtRegisteredClaimNames.Name] = user.Name,
                ["role"] = UserRoles.For(user)
            }
        };

        return new AccessToken(this.tokenHandler.CreateToken(descriptor), expiresAt);
    }
}
