#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IAuthenticationService
{
    bool IsValidLoginRequest(LoginRequestDto login);

    Task<Result<User>> AuthenticateUser(LoginRequestDto login);
    bool IsAuthenticated(string token);
}
