namespace Test.Modules.Shared;

public static class PasswordMother
{
    public static string Valid() => $"Aa1.{MotherCreator.Random.Random.AlphaNumeric(12)}";

    public static string Wrong() => $"Zz9!{MotherCreator.Random.Random.AlphaNumeric(12)}";
}
