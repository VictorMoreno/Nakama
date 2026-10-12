using AwesomeAssertions;
using Test.Modules.Shared;
using Test.Modules.Users.Domain;
using Nakama.Users.Application.Creator;
using Nakama.Users.Domain;
using Nakama.Users.Domain.Errors;

namespace Test.Modules.Users.Application;

[TestFixture]
public sealed class UserCreatorShould : UsersUnitTestCase
{
    private UserCreator.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UserCreator.Handler(new(this.Repository.Object, this.EventBus.Object, this.Clock.Object));
    }

    [Test]
    public async Task create_user_and_publish_user_created_when_email_is_available()
    {
        UserCreator.Command command = UserCreatorCommandMother.Random();
        this.ShouldFindUserByEmail(null);
        this.ShouldAcceptPassword();

        await this._handler.Handle(command, CancellationToken.None);

        this.ShouldHaveCreated(UserId.From(command.Id), command.InitialPassword);
        this.ShouldHavePublishedUserCreated(UserId.From(command.Id));
    }

    [Test]
    public async Task stamp_the_creation_date_with_the_time_given_by_the_clock()
    {
        DateTime now = DateTime.UtcNow;
        this.ShouldFindUserByEmail(null);
        this.ShouldAcceptPassword();
        this.ShouldBeNow(now);

        await this._handler.Handle(UserCreatorCommandMother.Random(), CancellationToken.None);

        this.ShouldHaveCreatedUserAt(now);
    }

    [Test]
    public async Task fail_without_creating_anything_when_email_is_already_in_use()
    {
        UserCreator.Command command = UserCreatorCommandMother.Random();
        this.ShouldFindUserByEmail(UserMother.WithEmail(new Email(command.Email)));

        Func<Task> act = async () => await this._handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserEmailAlreadyInUseError>();
        this.ShouldNotHaveCreatedAnyUser();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_creating_anything_when_the_email_is_not_valid()
    {
        UserCreator.Command command = UserCreatorCommandMother.With(email: WordMother.Random());

        Func<Task> act = async () => await this._handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UserInvalidEmailError>();
        this.ShouldNotHaveCreatedAnyUser();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_publishing_when_the_password_is_rejected()
    {
        string error = WordMother.Random();
        this.ShouldFindUserByEmail(null);
        this.ShouldRejectPassword(error);

        Func<Task> act = async () =>
            await this._handler.Handle(UserCreatorCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<UserInvalidError>().WithMessage($"*{error}*");
        this.ShouldNotHavePublishedAnything();
    }
}
