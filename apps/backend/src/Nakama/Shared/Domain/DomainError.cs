namespace Nakama.Shared.Domain;

public abstract class DomainError(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
