namespace Test.Modules.Shared;

public static class WordMother
{
    public static string Random() => MotherCreator.Random.Lorem.Word();
}
