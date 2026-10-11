using Bogus;

namespace Test.Modules.Shared;

public static class MotherCreator
{
    public static Faker Random { get; } = new();
}
