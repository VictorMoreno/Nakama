namespace Nakama.Authentication.Domain;

public interface ITokenGenerator
{
    Token Generate(Guid userId, string email, string name, bool isAdmin);
}
