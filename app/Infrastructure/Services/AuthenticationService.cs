#region

using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using zora.Core;
using zora.Core.DTOs;
using zora.Core.Interfaces;
using zora.Services.Configuration;

#endregion

namespace zora.Infrastructure.Services;

public sealed class AuthenticationService : IAuthenticationService, IZoraService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IUserService _userService;
    private readonly ISecretsManagerService _secretsManagerService;

    public AuthenticationService(IUserService userService, ILogger<AuthenticationService> logger,
        IConfiguration configuration, ISecretsManagerService secretsManagerService)
    {
        this._userService = userService;
        this._logger = logger;
        this._configuration = configuration;
        this._secretsManagerService = secretsManagerService;
    }

    public string GetJwt()
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        string? issuerSigningKey = this._secretsManagerService.GetSecret(Constants.IssuerSigningKey);
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
            if (this.isAuthenticated(login.Token))
            {
                this._logger.LogWarning("User {UserName} attempted to authenticate while already authenticated",
                    login.Username);
                throw new InvalidOperationException("User " + login.Username + " attempted to authenticate while already authenticated");
            }

            return await this._userService.ValidateUser(login);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error authenticating user {Username}", login.Username);
            throw;
        }
    }

    public bool isAuthenticated(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        string? issuerSigningKey = this._secretsManagerService.GetSecret(Constants.IssuerSigningKey);
        byte[] key = Encoding.UTF8.GetBytes(issuerSigningKey);
        TokenValidationParameters tokenValidationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        try
        {
            tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken _);
            return true;
        }
        catch(Exception ex)
        {
            this._logger.LogWarning(ex, "Invalid token provided");
            return false;
        }
    }
}
