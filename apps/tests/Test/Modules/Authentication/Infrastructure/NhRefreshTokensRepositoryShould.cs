using AwesomeAssertions;
using Nakama.Authentication.Domain;
using Nakama.Authentication.Infrastructure;
using Test.Modules.Authentication.Domain;
using Test.Modules.Shared.Infrastructure;

namespace Test.Modules.Authentication.Infrastructure;

[TestFixture]
public sealed class NhRefreshTokensRepositoryShould : InfrastructureTestCase
{
    private static readonly TimeSpan DatabasePrecision = TimeSpan.FromMilliseconds(1);

    private NhRefreshTokensRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        this._repository = new NhRefreshTokensRepository(this.WriteSessions);
    }

    [Test]
    public async Task find_a_saved_refresh_token_by_its_value()
    {
        RefreshToken token = RefreshTokenMother.Random();
        await this._repository.Save(token, CancellationToken.None);

        RefreshToken? found = await this._repository.FindByValue(token.Value!, CancellationToken.None);

        found.Should().NotBeNull();
        found.Id.Should().Be(token.Id);
        found.UserId.Should().Be(token.UserId);
        found.TokenHash.Should().Be(token.TokenHash);
        found.ExpiresAt.Should().BeCloseTo(token.ExpiresAt, DatabasePrecision);
        found.RevokedAt.Should().BeNull();
        found.ReplacedById.Should().BeNull();
    }

    [Test]
    public async Task find_nothing_when_no_refresh_token_has_that_value()
    {
        await this._repository.Save(RefreshTokenMother.Random(), CancellationToken.None);

        RefreshToken? found = await this._repository.FindByValue(RefreshTokenMother.RandomValue(), CancellationToken.None);

        found.Should().BeNull();
    }

    [Test]
    public async Task rotate_the_token_revoking_it_and_saving_its_replacement()
    {
        DateTime now = DateTime.UtcNow;
        RefreshToken current = RefreshTokenMother.Random();
        RefreshToken replacement = RefreshTokenMother.With(userId: current.UserId);
        await this._repository.Save(current, CancellationToken.None);

        bool rotated = await this._repository.Rotate(current, replacement, now, CancellationToken.None);

        rotated.Should().BeTrue();
        RefreshToken? storedCurrent = await this._repository.FindById(current.Id, CancellationToken.None);
        storedCurrent.Should().NotBeNull();
        storedCurrent.RevokedAt.Should().BeCloseTo(now, DatabasePrecision);
        storedCurrent.ReplacedById.Should().Be(replacement.Id.Value);
        RefreshToken? storedReplacement = await this._repository.FindById(replacement.Id, CancellationToken.None);
        storedReplacement.Should().NotBeNull();
        storedReplacement.UserId.Should().Be(current.UserId);
        storedReplacement.RevokedAt.Should().BeNull();
    }

    [Test]
    public async Task refuse_to_rotate_a_token_that_was_already_rotated_and_save_no_second_replacement()
    {
        RefreshToken current = RefreshTokenMother.Random();
        RefreshToken firstReplacement = RefreshTokenMother.With(userId: current.UserId);
        RefreshToken secondReplacement = RefreshTokenMother.With(userId: current.UserId);
        await this._repository.Save(current, CancellationToken.None);
        await this._repository.Rotate(current, firstReplacement, DateTime.UtcNow, CancellationToken.None);

        bool rotated = await this._repository.Rotate(current, secondReplacement, DateTime.UtcNow, CancellationToken.None);

        rotated.Should().BeFalse();
        RefreshToken? storedCurrent = await this._repository.FindById(current.Id, CancellationToken.None);
        storedCurrent.Should().NotBeNull();
        storedCurrent.ReplacedById.Should().Be(firstReplacement.Id.Value);
        (await this._repository.FindById(secondReplacement.Id, CancellationToken.None)).Should().BeNull();
    }

    [Test]
    public async Task refuse_to_rotate_a_token_that_was_revoked_and_save_no_replacement()
    {
        RefreshToken current = RefreshTokenMother.Revoked();
        RefreshToken replacement = RefreshTokenMother.With(userId: current.UserId);
        await this._repository.Save(current, CancellationToken.None);

        bool rotated = await this._repository.Rotate(current, replacement, DateTime.UtcNow, CancellationToken.None);

        rotated.Should().BeFalse();
        (await this._repository.FindById(replacement.Id, CancellationToken.None)).Should().BeNull();
    }

    [Test]
    public async Task let_only_one_of_several_concurrent_rotations_win()
    {
        RefreshToken current = RefreshTokenMother.Random();
        List<RefreshToken> replacements = Enumerable.Range(0, 5)
            .Select(_ => RefreshTokenMother.With(userId: current.UserId))
            .ToList();
        await this._repository.Save(current, CancellationToken.None);

        bool[] results = await Task.WhenAll(replacements.Select(replacement =>
            this._repository.Rotate(current, replacement, DateTime.UtcNow, CancellationToken.None)));

        results.Count(rotated => rotated).Should().Be(1);
        RefreshToken?[] stored = await Task.WhenAll(replacements.Select(replacement =>
            this._repository.FindById(replacement.Id, CancellationToken.None)));
        stored.Count(token => token is not null).Should().Be(1);
    }

    [Test]
    public async Task revoke_only_the_active_tokens_of_the_user()
    {
        DateTime now = DateTime.UtcNow;
        Guid userId = Guid.NewGuid();
        RefreshToken first = RefreshTokenMother.With(userId: userId);
        RefreshToken second = RefreshTokenMother.With(userId: userId);
        RefreshToken alreadyRevoked = RefreshTokenMother.With(userId: userId, revokedAt: now.AddHours(-2));
        RefreshToken ofAnotherUser = RefreshTokenMother.Random();
        foreach (RefreshToken token in new[] { first, second, alreadyRevoked, ofAnotherUser })
        {
            await this._repository.Save(token, CancellationToken.None);
        }

        await this._repository.RevokeAllOf(userId, now, CancellationToken.None);

        (await this._repository.FindById(first.Id, CancellationToken.None))!.RevokedAt.Should().BeCloseTo(now, DatabasePrecision);
        (await this._repository.FindById(second.Id, CancellationToken.None))!.RevokedAt.Should().BeCloseTo(now, DatabasePrecision);
        (await this._repository.FindById(alreadyRevoked.Id, CancellationToken.None))!.RevokedAt
            .Should().BeCloseTo(alreadyRevoked.RevokedAt!.Value, DatabasePrecision);
        (await this._repository.FindById(ofAnotherUser.Id, CancellationToken.None))!.RevokedAt.Should().BeNull();
    }
}
