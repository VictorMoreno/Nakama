using AwesomeAssertions;
using Moq;
using Nakama.Shared.Domain.Specification;
using Nakama.Users.Application;
using Nakama.Users.Application.CountFinder;
using Nakama.Users.Domain;

namespace Test.Modules.Users.Application;

[TestFixture]
public sealed class UsersCountFinderShould : UsersUnitTestCase
{
    private UsersCountFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UsersCountFinder.Handler(new(this.Repository.Object));
    }

    [TestCase(0)]
    [TestCase(20)]
    public async Task return_how_many_users_there_are(int count)
    {
        this.Repository
            .Setup(repository => repository.Count(
                It.Is<ISpecification<User>>(specification => specification is UserListingSpecification),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

        UsersCountResponse response = await this._handler.Handle(new UsersCountFinder.Query(), CancellationToken.None);

        response.Total.Should().Be(count);
    }
}
