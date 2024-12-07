#region

using zora.Core.DTOs;

#endregion

namespace zora.Core.Interfaces;

public interface IAuthenticationService
{
    string GetJwt();
    bool IsValidLoginRequest(LoginRequestDto login);

    Task<bool> AuthenticateUser(LoginRequestDto login);
    bool isAuthenticated(string token);
}
