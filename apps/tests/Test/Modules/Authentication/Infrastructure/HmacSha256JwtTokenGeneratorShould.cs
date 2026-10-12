using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Nakama.Authentication.Domain;
using Nakama.Authentication.Infrastructure;
using Nakama.Shared.Infrastructure.Auth;
using System.Text;
using Test.Modules.Authentication.Domain;
using Test.Modules.Shared;
using Test.Modules.Users.Domain;

namespace Test.Modules.Authentication.Infrastructure;

[TestFixture]
public sealed class HmacSha256JwtTokenGeneratorShould : UnitTestCase
{
    private readonly JsonWebTokenHandler _handler = new();

    private JwtOptions _options = null!;

    private HmacSha256JwtTokenGenerator _generator = null!;

    [SetUp]
    public void SetUp()
    {
        this._options = JwtOptionsMother.Random();
        this._generator = new HmacSha256JwtTokenGenerator(Options.Create(this._options), this.Clock.Object);
    }

    [Test]
    public async Task issue_a_token_signed_with_hmac_sha256_that_validates_against_the_signing_key()
    {
        this.ShouldBeNow(DateTime.UtcNow);
        Token token = this._generator.Generate(
            Guid.NewGuid(),
            UserMother.RandomEmail().Value,
            WordMother.Random(),
            false);

        TokenValidationResult result = await this._handler.ValidateTokenAsync(
            token.Value,
            new TokenValidationParameters
            {
                ValidIssuer = this._options.Issuer,
                ValidAudience = this._options.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._options.SigningKey)),
                ClockSkew = TimeSpan.Zero
            });

        result.IsValid.Should().BeTrue();
        this._handler.ReadJsonWebToken(token.Value).Alg.Should().Be(SecurityAlgorithms.HmacSha256);
    }

    [Test]
    public async Task issue_a_token_that_does_not_validate_against_another_signing_key()
    {
        string anotherSigningKey = JwtOptionsMother.Random().SigningKey;
        this.ShouldBeNow(DateTime.UtcNow);
        Token token = this._generator.Generate(
            Guid.NewGuid(),
            UserMother.RandomEmail().Value,
            WordMother.Random(),
            false);

        TokenValidationResult result = await this._handler.ValidateTokenAsync(
            token.Value,
            new TokenValidationParameters
            {
                ValidIssuer = this._options.Issuer,
                ValidAudience = this._options.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(anotherSigningKey)),
                ClockSkew = TimeSpan.Zero
            });

        result.IsValid.Should().BeFalse();
    }

    [Test]
    public void include_the_identity_of_the_user_in_the_token()
    {
        Guid userId = UserMother.RandomId().Value;
        string email = UserMother.RandomEmail().Value;
        string name = WordMother.Random();
        this.ShouldBeNow(DateTime.UtcNow);

        Token token = this._generator.Generate(userId, email, name, false);

        JsonWebToken jwt = this._handler.ReadJsonWebToken(token.Value);
        jwt.Subject.Should().Be(userId.ToString());
        jwt.GetPayloadValue<string>(JwtRegisteredClaimNames.Email).Should().Be(email);
        jwt.GetPayloadValue<string>(JwtRegisteredClaimNames.Name).Should().Be(name);
    }

    [Test]
    public void grant_the_admin_role_only_to_administrators()
    {
        this.ShouldBeNow(DateTime.UtcNow);

        Token administrator = this._generator.Generate(Guid.NewGuid(), WordMother.Random(), WordMother.Random(), true);
        Token member = this._generator.Generate(Guid.NewGuid(), WordMother.Random(), WordMother.Random(), false);

        this._handler.ReadJsonWebToken(administrator.Value).GetPayloadValue<string>("role")
            .Should().Be(AuthRoles.Admin);
        this._handler.ReadJsonWebToken(member.Value).GetPayloadValue<string>("role")
            .Should().Be(AuthRoles.Member);
    }

    [Test]
    public void expire_after_the_configured_minutes()
    {
        DateTime now = DateTime.UtcNow;
        this.ShouldBeNow(now);

        Token token = this._generator.Generate(
            Guid.NewGuid(),
            UserMother.RandomEmail().Value,
            WordMother.Random(),
            false);

        token.ExpiresAt.Should().Be(now.AddMinutes(this._options.AccessTokenMinutes));
        this._handler.ReadJsonWebToken(token.Value).ValidTo
            .Should().BeCloseTo(token.ExpiresAt, TimeSpan.FromSeconds(1));
    }

    [Test]
    public void give_every_token_a_different_identifier()
    {
        Guid userId = UserMother.RandomId().Value;
        string email = UserMother.RandomEmail().Value;
        string name = WordMother.Random();
        this.ShouldBeNow(DateTime.UtcNow);

        Token first = this._generator.Generate(userId, email, name, false);
        Token second = this._generator.Generate(userId, email, name, false);

        this._handler.ReadJsonWebToken(first.Value).Id
            .Should().NotBe(this._handler.ReadJsonWebToken(second.Value).Id);
    }
}
