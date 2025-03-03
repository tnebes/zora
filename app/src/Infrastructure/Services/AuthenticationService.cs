#region

using System.IdentityModel.Tokens.Jwt;
using FluentResults;
using Microsoft.IdentityModel.Tokens;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;
using zora.Extensions;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class AuthenticationService : IAuthenticationService, IZoraService
{
    private readonly ILogger<AuthenticationService> _logger;
    private readonly ISecretsManagerService _secretsManagerService;
    private readonly IUserService _userService;

    public AuthenticationService(IUserService userService, ILogger<AuthenticationService> logger,
        ISecretsManagerService secretsManagerService)
    {
        this._userService = userService;
        this._logger = logger;
        this._secretsManagerService = secretsManagerService;
    }

    public bool IsValidLoginRequest(LoginRequestDto login) =>
        !string.IsNullOrWhiteSpace(login.Username) && !string.IsNullOrWhiteSpace(login.Password);

    public async Task<Result<User>> AuthenticateUser(LoginRequestDto login)
    {
        try
        {
            if (this.IsAuthenticated(login.Token))
            {
                this._logger.LogWarning("User {UserName} attempted to authenticate while already authenticated",
                    login.Username);
                return Result.Fail<User>(new Error("User is already authenticated")
                    .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.UserAlreadyAuthenticated));
            }

            Result<User> userResult = await this._userService.ValidateUser(login);

            if (userResult.IsFailed)
            {
                this._logger.LogInformation("User {Username} failed to authenticate", login.Username);
                return Result.Fail<User>(new Error("Invalid credentials")
                    .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.InvalidCredentials));
            }

            User user = userResult.Value;

            if (user.Deleted)
            {
                this._logger.LogInformation("Deleted user {Username} attempted to authenticate", login.Username);
                return Result.Fail<User>(new Error("User account has been deleted")
                    .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.UserDeleted));
            }

            return user;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error authenticating user {Username}", login.Username);
            return Result.Fail<User>(new Error("Error authenticating user")
                .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.AuthenticationError));
        }
    }

    public bool IsAuthenticated(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        string? issuerSigningKey = this._secretsManagerService.GetSecret(Constants.ISSUER_SIGNING_KEY);
        TokenValidationParameters tokenValidationParameters =
            TokenValidationExtensions.CreateTokenValidationParameters(issuerSigningKey);

        try
        {
            tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken _);
            return true;
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(ex, "Invalid token provided");
            return false;
        }
    }
}
