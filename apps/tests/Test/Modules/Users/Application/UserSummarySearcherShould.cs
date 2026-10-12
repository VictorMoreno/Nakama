using AwesomeAssertions;
using Nakama.Users.Application;
using Nakama.Users.Application.Searcher;
using Nakama.Users.Domain;
using Test.Modules.Users.Domain;

namespace Test.Modules.Users.Application;

[TestFixture]
public sealed class UserSummarySearcherShould : UsersUnitTestCase
{
    private UserSummarySearcher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UserSummarySearcher.Handler(new(this.Repository.Object));
    }

    [Test]
    public async Task return_a_summary_of_every_user_in_the_same_order()
    {
        User first = UserMother.Random();
        User second = UserMother.Random();
        this.ShouldFindAllUsers([first, second]);

        IReadOnlyCollection<UserSummaryResponse> response = await this._handler.Handle(
            new UserSummarySearcher.Query(),
            CancellationToken.None);

        response.Should().Equal(
            new UserSummaryResponse(first.Id.Value, first.Name, first.Email),
            new UserSummaryResponse(second.Id.Value, second.Name, second.Email));
    }

    [Test]
    public async Task return_an_empty_collection_when_there_are_no_users()
    {
        this.ShouldFindAllUsers([]);

        IReadOnlyCollection<UserSummaryResponse> response = await this._handler.Handle(
            new UserSummarySearcher.Query(),
            CancellationToken.None);

        response.Should().BeEmpty();
    }
}
