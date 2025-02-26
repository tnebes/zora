#region

using zora.Core.Domain;
using zora.Core.Enums;

#endregion

namespace zora.Core.DTOs;

public sealed class AuthenticationResult
{
    public User? User { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public AuthenticationErrorType ErrorType { get; init; }
}
