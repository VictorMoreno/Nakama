using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nakama.Authentication.Domain;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Auth;

namespace Nakama.Authentication.Infrastructure;

public sealed class HmacSha256JwtTokenGenerator : ITokenGenerator
{
    private readonly JwtOptions options;
    private readonly ISystemClock systemClock;
    private readonly SigningCredentials signingCredentials;
    private readonly JsonWebTokenHandler tokenHandler = new();

    public HmacSha256JwtTokenGenerator(IOptions<JwtOptions> options, ISystemClock systemClock)
    {
        this.options = options.Value;
        this.systemClock = systemClock;
        this.signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public Token Generate(Guid userId, string email, string name, bool isAdmin)
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
                [JwtRegisteredClaimNames.Sub] = userId.ToString(),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                [JwtRegisteredClaimNames.Email] = email,
                [JwtRegisteredClaimNames.Name] = name,
                ["role"] = AuthRoles.For(isAdmin)
            }
        };

        return new Token(this.tokenHandler.CreateToken(descriptor), expiresAt);
    }
}
