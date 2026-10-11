namespace Nakama.Authentication.Domain;

public static class AuthRoles
{
    public const string Admin = "admin";

    public const string Member = "member";

    public static string For(bool isAdmin) => isAdmin ? Admin : Member;
}
