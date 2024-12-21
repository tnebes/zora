namespace zora.Core.Enums;

public enum AuthenticationErrorType
{
    None,
    InvalidCredentials,
    UserNotFound,
    ValidationError,
    SystemError,
    UserAlreadyAuthenticated,
    AuthenticationError
}
