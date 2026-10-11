using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nakama.Shared.Infrastructure.Auth;
using Nakama.Users.Domain;
using Nakama.Users.Infrastructure;

namespace Api.Configuration;

public static class AuthExtensions
{
    private const string RoleClaimType = "role";
    private const string NameClaimType = "name";

    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer must not be empty.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience must not be empty.")
            .Validate(
                options => !string.IsNullOrEmpty(options.SigningKey) &&
                           Encoding.UTF8.GetByteCount(options.SigningKey) >= JwtOptions.MinSigningKeyBytes,
                $"Jwt:SigningKey is missing or shorter than {JwtOptions.MinSigningKeyBytes} bytes. Set it with 'dotnet user-secrets' or the Jwt__SigningKey environment variable.")
            .Validate(options => options.AccessTokenMinutes > 0, "Jwt:AccessTokenMinutes must be greater than zero.")
            .Validate(options => options.RefreshTokenDays > 0, "Jwt:RefreshTokenDays must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                JwtOptions jwt = jwtOptions.Value;

                bearer.MapInboundClaims = false;
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = NameClaimType,
                    RoleClaimType = RoleClaimType
                };
            });

        services.AddAuthorization();

        return services;
    }
}
