#region

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using zora.Core;
using zora.Core.DTOs;
using zora.Core.Interfaces;

#endregion

namespace zora.Infrastructure.Services;

public sealed class AuthenticationService : IAuthenticationService, IZoraService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IUserService _userService;

    public AuthenticationService(IUserService userService, ILogger<AuthenticationService> logger)
    {
        this._userService = userService;
        this._logger = logger;
    }

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

    public bool IsValidLoginRequest(LoginRequest login) =>
        !string.IsNullOrWhiteSpace(login.Username) && !string.IsNullOrWhiteSpace(login.Password);

    public async Task<bool> AuthenticateUser(LoginRequest login)
    {
        try
        {
            return await this._userService.ValidateUser(login);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error authenticating user {Username}", login.Username);
            throw;
        }
    }
}
