using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using zora.Core;
using zora.Core.DTOs;
using zora.Core.Interfaces;

namespace zora.Infrastructure.Services;

public sealed class AuthenticationService : IAuthenticationService, IZoraService
{
    public string GetJwt()
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.UTF8.GetBytes(Constants.IssuerSigningKey);

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256)
        };

        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        string? jwt = tokenHandler.WriteToken(token);
        return jwt;
    }

    public bool IsValidLoginRequest(LoginRequest login) => !string.IsNullOrWhiteSpace(login.Username)
                                                           && !string.IsNullOrWhiteSpace(login.Password);

    public bool AuthenticateUser(LoginRequest login) => login.Username != "tnebes" || login.Password != "letmeinside1";
}
