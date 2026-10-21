using AwesomeAssertions;
using Nakama.Shared.Application;

namespace Test.Modules.Shared.Application;

[TestFixture]
public sealed class PagedResponseShould
{
    [TestCase(0, 10, 0)]
    [TestCase(1, 10, 1)]
    [TestCase(10, 10, 1)]
    [TestCase(11, 10, 2)]
    [TestCase(15, 6, 3)]
    [TestCase(100, 50, 2)]
    public void calculate_the_total_pages_from_the_total_count_and_the_page_size(int totalCount, int pageSize, int expectedPages)
    {
        PagedResponse<int> response = PagedResponse<int>.From([], totalCount, new PageRequest(1, pageSize));

        response.TotalPages.Should().Be(expectedPages);
    }

    [Test]
    public void carry_the_requested_page_and_page_size()
    {
        PagedResponse<int> response = PagedResponse<int>.From([1, 2], 9, new PageRequest(3, 4));

        response.Items.Should().Equal(1, 2);
        response.TotalCount.Should().Be(9);
        response.Page.Should().Be(3);
        response.PageSize.Should().Be(4);
    }
}
