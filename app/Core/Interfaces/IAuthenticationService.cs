using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using zora.Core.DTOs;

namespace zora.Core.Interfaces;

public interface IAuthenticationService
{
    string GetJwt();
    bool IsValidLoginRequest(LoginRequest login);

    bool AuthenticateUser(LoginRequest login);
}
