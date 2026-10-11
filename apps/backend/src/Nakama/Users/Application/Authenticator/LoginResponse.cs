namespace Nakama.Users.Application.Authenticator;

public sealed record LoginResponse(string AccessToken, DateTime ExpiresAt, UserResponse User);
