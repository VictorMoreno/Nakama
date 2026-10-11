using Nakama.Shared.Domain;

namespace Nakama.Users.Domain.Errors;

public sealed class UserEmailAlreadyInUseError(string email)
    : DomainError("nakama.users.already_exists", $"Email '{email}' is already in use.");

public sealed class UserInvalidCredentialsError()
    : DomainError("nakama.users.invalid_credentials", "Invalid email or password.");

public sealed class UserNotFoundError(string id)
    : DomainError("nakama.users.not_found", $"User '{id}' was not found.");

public sealed class UserInvalidError(string details)
    : DomainError("nakama.users.invalid", $"Could not create or update user: {details}");

public sealed class UserInvalidEmailError(string value)
    : DomainError("nakama.users.invalid_email", $"'{value}' is not a valid email address.");

public static class UserErrors
{
    public static DomainError EmailAlreadyInUse(string email) => new UserEmailAlreadyInUseError(email);

    public static DomainError InvalidCredentials { get; } = new UserInvalidCredentialsError();

    public static DomainError NotFound(string id) => new UserNotFoundError(id);

    public static DomainError Invalid(string details) => new UserInvalidError(details);

    public static DomainError InvalidEmail(string value) => new UserInvalidEmailError(value);
}
