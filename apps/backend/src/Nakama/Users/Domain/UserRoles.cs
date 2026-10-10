namespace Nakama.Users.Domain;

public static class UserRoles
{
    public const string Admin = "admin";

    public const string Member = "member";

    public static string For(User user) => user.IsAdmin ? Admin : Member;
}
