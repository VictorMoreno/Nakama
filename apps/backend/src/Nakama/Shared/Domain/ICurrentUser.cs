namespace Nakama.Shared.Domain;

public interface ICurrentUser
{
    Guid Id { get; }

    bool IsAdmin { get; }
}
