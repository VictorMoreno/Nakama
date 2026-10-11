namespace Nakama.Authentication.Domain;

public sealed record Token(string Value, DateTime ExpiresAt);
