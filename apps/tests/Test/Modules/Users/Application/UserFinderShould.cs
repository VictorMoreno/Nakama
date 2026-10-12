using AwesomeAssertions;
using Nakama.Users.Application;
using Nakama.Users.Application.Finder;
using Nakama.Users.Domain;
using Nakama.Users.Domain.Errors;
using Test.Modules.Users.Domain;

namespace Test.Modules.Users.Application;

[TestFixture]
public sealed class UserFinderShould : UsersUnitTestCase
{
    private UserFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UserFinder.Handler(new(this.Repository.Object));
    }

    [Test]
    public async Task return_the_user_as_a_response_when_it_exists()
    {
        User user = UserMother.WithAdmin();
        this.ShouldFindUser(user);

        UserResponse response = await this._handler.Handle(
            UserFinderQueryMother.With(user.Id.Value),
            CancellationToken.None);

        response.Id.Should().Be(user.Id.Value);
        response.Email.Should().Be(user.Email);
        response.Name.Should().Be(user.Name);
        response.IsAdmin.Should().BeTrue();
    }

    [Test]
    public async Task fail_when_the_user_does_not_exist()
    {
        this.ShouldFindUser(null);

        Func<Task> act = async () => await this._handler.Handle(UserFinderQueryMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundError>();
    }
}
