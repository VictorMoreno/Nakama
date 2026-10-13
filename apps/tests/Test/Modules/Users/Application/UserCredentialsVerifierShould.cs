using AwesomeAssertions;
using Nakama.Users.Application;
using Nakama.Users.Application.CredentialsVerifier;
using Nakama.Users.Domain;
using Nakama.Users.Domain.Errors;
using Test.Modules.Shared;
using Test.Modules.Users.Domain;

namespace Test.Modules.Users.Application;

[TestFixture]
public sealed class UserCredentialsVerifierShould : UsersUnitTestCase
{
    private UserCredentialsVerifier.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UserCredentialsVerifier.Handler(
            new(this.Repository.Object, this.EventBus.Object, this.Clock.Object));
    }

    [Test]
    public async Task return_the_user_when_the_credentials_are_valid()
    {
        User user = UserMother.WithAdmin();
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindUserByEmail(user);
        this.ShouldVerifyPassword(true);

        UserResponse response = await this._handler.Handle(
            UserCredentialsVerifierQueryMother.With(user.Email),
            CancellationToken.None);

        response.Id.Should().Be(user.Id.Value);
        response.Email.Should().Be(user.Email);
        response.Name.Should().Be(user.Name);
        response.IsAdmin.Should().BeTrue();
    }

    [Test]
    public async Task mark_the_user_as_active_at_the_time_of_the_clock_and_publish_the_activity()
    {
        DateTime now = DateTime.UtcNow;
        User user = UserMother.Random();
        this.ShouldBeNow(now);
        this.ShouldFindUserByEmail(user);
        this.ShouldVerifyPassword(true);

        await this._handler.Handle(UserCredentialsVerifierQueryMother.With(user.Email), CancellationToken.None);

        user.LastActiveAt.Should().Be(now);
        this.ShouldHaveSavedUser(user);
        this.ShouldHavePublishedUserActivityRecorded(user.Id, now);
    }

    [Test]
    public async Task fail_without_checking_the_password_when_the_email_is_not_registered()
    {
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldNotFindUserByEmail();

        Func<Task> act = async () =>
            await this._handler.Handle(UserCredentialsVerifierQueryMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<UserInvalidCredentialsError>();
        this.ShouldNotHaveVerifiedAnyPassword();
        this.ShouldNotHaveSavedAnyUser();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_recording_activity_when_the_password_is_wrong()
    {
        User user = UserMother.Random();
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindUserByEmail(user);
        this.ShouldVerifyPassword(false);

        Func<Task> act = async () => await this._handler.Handle(
            UserCredentialsVerifierQueryMother.With(user.Email),
            CancellationToken.None);

        await act.Should().ThrowAsync<UserInvalidCredentialsError>();
        user.LastActiveAt.Should().BeNull();
        this.ShouldNotHaveSavedAnyUser();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_looking_up_anything_when_the_email_is_not_valid()
    {
        this.ShouldBeNow(DateTime.UtcNow);

        Func<Task> act = async () => await this._handler.Handle(
            UserCredentialsVerifierQueryMother.With(WordMother.Random()),
            CancellationToken.None);

        await act.Should().ThrowAsync<UserInvalidEmailError>();
        this.ShouldNotHaveVerifiedAnyPassword();
        this.ShouldNotHaveSavedAnyUser();
        this.ShouldNotHavePublishedAnything();
    }
}
