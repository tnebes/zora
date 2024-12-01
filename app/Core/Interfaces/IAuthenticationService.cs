#region

using zora.Core.DTOs;

#endregion

namespace zora.Core.Interfaces;

public interface IAuthenticationService
{
    string GetJwt();
    bool IsValidLoginRequest(LoginRequest login);

    Task<bool> AuthenticateUser(LoginRequest login);
}
