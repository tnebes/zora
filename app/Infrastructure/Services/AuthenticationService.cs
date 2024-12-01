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
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IUserService _userService;

    public AuthenticationService(IUserService userService, ILogger<AuthenticationService> logger,
        IConfiguration configuration)
    {
        this._userService = userService;
        this._logger = logger;
        this._configuration = configuration;
    }

    public string GetJwt()
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        string? issuerSigningKey = this._configuration[Constants.IssuerSigningKey];

        if (string.IsNullOrWhiteSpace(issuerSigningKey))
        {
            this._logger.LogError("{KeyName} not found in configuration. Use dotnet user-secrets.",
                Constants.IssuerSigningKey);
            throw new InvalidOperationException(
                Constants.IssuerSigningKey + " not found in environment variables. Use dotnet user-secrets.");
        }

        byte[] key = Encoding.UTF8.GetBytes(issuerSigningKey);
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
