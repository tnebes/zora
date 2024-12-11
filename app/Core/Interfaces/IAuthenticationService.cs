#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs;

#endregion

namespace zora.Core.Interfaces;

public interface IAuthenticationService
{
    bool IsValidLoginRequest(LoginRequestDto login);

    Task<Result<User>> AuthenticateUser(LoginRequestDto login);
    bool IsAuthenticated(string token);
}
