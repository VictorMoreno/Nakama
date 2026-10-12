using Microsoft.Extensions.Options;
using Moq;
using Nakama.Authentication.Domain;
using Nakama.Shared.Infrastructure.Auth;
using Nakama.Shared.Domain;
using Nakama.Users.Application;
using Nakama.Users.Application.CredentialsVerifier;
using Nakama.Users.Application.Finder;
using Nakama.Users.Domain.Errors;
using Test.Modules.Authentication.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Authentication;

public abstract class AuthenticationUnitTestCase : UnitTestCase
{
    protected Mock<IQueryBus> QueryBus { get; } = new();

    protected Mock<IRefreshTokensRepository> RefreshTokens { get; } = new();

    protected Mock<ITokenGenerator> TokenGenerator { get; } = new();

    protected IOptions<JwtOptions> Jwt { get; } = Options.Create(JwtOptionsMother.Random());

    [SetUp]
    public void AuthenticationSetUp()
    {
        this.QueryBus.Reset();
        this.RefreshTokens.Reset();
        this.TokenGenerator.Reset();
    }

    protected void ShouldVerifyCredentials(UserResponse user) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<UserCredentialsVerifier.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    protected void ShouldRejectCredentials() =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<UserCredentialsVerifier.Query>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(UserErrors.InvalidCredentials);

    protected void ShouldFindUser(UserResponse user) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<UserFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

    protected void ShouldNotFindUser(Guid id) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<UserFinder.Query>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(UserErrors.NotFound(id.ToString()));

    protected void ShouldFindRefreshToken(RefreshToken? token) =>
        this.RefreshTokens
            .Setup(repository => repository.FindByValue(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

    protected void ShouldGenerateAccessToken(Token token) =>
        this.TokenGenerator
            .Setup(generator => generator.Generate(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .Returns(token);

    protected void ShouldRotate(bool succeeded) =>
        this.RefreshTokens
            .Setup(repository => repository.Rotate(
                It.IsAny<RefreshToken>(),
                It.IsAny<RefreshToken>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(succeeded);

    protected void ShouldHaveGeneratedAccessTokenFor(UserResponse user) =>
        this.TokenGenerator.Verify(
            generator => generator.Generate(user.Id, user.Email, user.Name, user.IsAdmin),
            Times.Once);

    protected void ShouldNotHaveGeneratedAnyAccessToken() =>
        this.TokenGenerator.Verify(
            generator => generator.Generate(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()),
            Times.Never);

    protected void ShouldHaveSaved(RefreshToken token) =>
        this.RefreshTokens.Verify(
            repository => repository.Save(token, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveSavedRefreshToken(string value, Guid userId, DateTime expiresAt) =>
        this.RefreshTokens.Verify(
            repository => repository.Save(
                It.Is<RefreshToken>(token =>
                    token.Value == value && token.UserId == userId && token.ExpiresAt == expiresAt),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSavedAnyRefreshToken() =>
        this.RefreshTokens.Verify(
            repository => repository.Save(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveRotated(RefreshToken current) =>
        this.RefreshTokens.Verify(
            repository => repository.Rotate(
                current,
                It.Is<RefreshToken>(replacement =>
                    replacement.UserId == current.UserId && replacement.Value != current.Value),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveRotated() =>
        this.RefreshTokens.Verify(
            repository => repository.Rotate(
                It.IsAny<RefreshToken>(),
                It.IsAny<RefreshToken>(),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveRevokedAllTokensOf(Guid userId) =>
        this.RefreshTokens.Verify(
            repository => repository.RevokeAllOf(userId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveRevokedAnyToken() =>
        this.RefreshTokens.Verify(
            repository => repository.RevokeAllOf(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
