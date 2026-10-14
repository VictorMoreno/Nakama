using AwesomeAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Users.Domain;
using Nakama.Users.Infrastructure;
using Test.Modules.Shared.Infrastructure;
using Test.Modules.Users.Domain;

namespace Test.Modules.Users.Infrastructure;

[TestFixture]
public sealed class NhUsersRepositoryShould : InfrastructureTestCase
{
    private static readonly TimeSpan DatabasePrecision = TimeSpan.FromMilliseconds(1);

    private NhUsersRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton(this.WriteSessions);
        services.AddIdentityCore<User>().AddUserStore<NhUserRepository>();
        UserManager<User> userManager = services.BuildServiceProvider().GetRequiredService<UserManager<User>>();

        this._repository = new NhUsersRepository(this.WriteSessions, userManager);
    }

    [Test]
    public async Task keep_every_field_of_a_saved_user()
    {
        DateTime now = DateTime.UtcNow;
        User user = UserMother.With(
            isAdmin: true,
            passwordHash: "hashed-password",
            lastActiveAt: now.AddHours(-3),
            createdAt: now.AddDays(-30));
        await this._repository.Save(user, CancellationToken.None);

        User? found = await this._repository.FindById(user.Id, CancellationToken.None);

        found.Should().NotBeNull();
        found.Email.Should().Be(user.Email);
        found.NormalizedEmail.Should().Be(user.NormalizedEmail);
        found.Name.Should().Be(user.Name);
        found.IsAdmin.Should().BeTrue();
        found.PasswordHash.Should().Be("hashed-password");
        found.LastActiveAt.Should().BeCloseTo(user.LastActiveAt!.Value, DatabasePrecision);
        found.CreatedAt.Should().BeCloseTo(user.CreatedAt, DatabasePrecision);
    }

    [Test]
    public async Task find_a_user_by_its_email_whatever_the_case()
    {
        User user = UserMother.With(email: new Email("Ada.Lovelace@Nakama.Test"));
        await this._repository.Save(user, CancellationToken.None);
        await this._repository.Save(UserMother.Random(), CancellationToken.None);

        User? found = await this._repository.GetByEmail(new Email("ada.lovelace@nakama.test"), CancellationToken.None);

        found.Should().NotBeNull();
        found.Id.Should().Be(user.Id);
    }

    [Test]
    public async Task find_nothing_when_no_user_has_that_email()
    {
        await this._repository.Save(UserMother.Random(), CancellationToken.None);

        User? found = await this._repository.GetByEmail(UserMother.RandomEmail(), CancellationToken.None);

        found.Should().BeNull();
    }

    [Test]
    public async Task list_the_users_ordered_by_name()
    {
        await this._repository.Save(UserMother.With(name: "Carla"), CancellationToken.None);
        await this._repository.Save(UserMother.With(name: "Ana"), CancellationToken.None);
        await this._repository.Save(UserMother.With(name: "Bruno"), CancellationToken.None);

        IReadOnlyList<User> users = await this._repository.FindAll(CancellationToken.None);

        users.Select(user => user.Name).Should().Equal("Ana", "Bruno", "Carla");
    }

    [Test]
    public async Task list_no_users_when_there_are_none()
    {
        IReadOnlyList<User> users = await this._repository.FindAll(CancellationToken.None);

        users.Should().BeEmpty();
    }

    [Test]
    public async Task count_every_saved_user()
    {
        await this._repository.Save(UserMother.Random(), CancellationToken.None);
        await this._repository.Save(UserMother.Random(), CancellationToken.None);
        await this._repository.Save(UserMother.Random(), CancellationToken.None);

        int count = await this._repository.CountAll(CancellationToken.None);

        count.Should().Be(3);
    }

    [Test]
    public async Task count_no_users_when_there_are_none()
    {
        int count = await this._repository.CountAll(CancellationToken.None);

        count.Should().Be(0);
    }

    [Test]
    public async Task count_only_the_users_active_since_the_cutoff_including_the_cutoff_itself()
    {
        DateTime cutoff = DateTime.UtcNow.AddDays(-7);
        cutoff = new DateTime(cutoff.Year, cutoff.Month, cutoff.Day, cutoff.Hour, cutoff.Minute, cutoff.Second, DateTimeKind.Utc);
        await this._repository.Save(UserMother.With(lastActiveAt: cutoff.AddHours(1)), CancellationToken.None);
        await this._repository.Save(UserMother.With(lastActiveAt: cutoff), CancellationToken.None);
        await this._repository.Save(UserMother.With(lastActiveAt: cutoff.AddHours(-1)), CancellationToken.None);
        await this._repository.Save(UserMother.With(lastActiveAt: null), CancellationToken.None);

        int count = await this._repository.CountActiveSince(cutoff, CancellationToken.None);

        count.Should().Be(2);
    }
}
