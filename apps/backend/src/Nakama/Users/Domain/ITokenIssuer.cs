namespace Nakama.Users.Domain;

public interface ITokenIssuer
{
    AccessToken IssueAccessToken(User user);
}

public sealed record AccessToken(string Value, DateTime ExpiresAt);
