using Nakama.Users.Application.Finder;
using Test.Modules.Users.Domain;

namespace Test.Modules.Users.Application;

public static class UserFinderQueryMother
{
    public static UserFinder.Query Random() => With();

    public static UserFinder.Query With(Guid? id = null) => new(id ?? UserMother.RandomId().Value);
}
