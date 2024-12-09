#region

using FluentResults;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.Enums;
using zora.Core.Interfaces;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class UserService : IUserService, IZoraService
{
    private readonly ILogger<UserService> _logger;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        this._userRepository = userRepository;
        this._logger = logger;
    }

    public async Task<Result<User>> ValidateUser(LoginRequestDto login)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(login.Username) || string.IsNullOrWhiteSpace(login.Password))
            {
                return Result.Fail<User>(new Error("Invalid credentials provided")
                    .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.ValidationError));
            }

            Result<User> user = await this.GetUserByUsernameAsync(login.Username);

            if (user.IsFailed)
            {
                return Result.Fail<User>(new Error("User not found")
                    .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.UserNotFound));
            }

            string hashedPassword = UserService.HashPassword(login.Password);
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(login.Password, hashedPassword);

            if (!isPasswordValid)
            {
                return Result.Fail<User>(new Error("Invalid credentials")
                    .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.InvalidCredentials));
            }

            return Result.Ok(user.Value);
        }
        catch (KeyNotFoundException)
        {
            return Result.Fail<User>(new Error("User not found")
                .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.UserNotFound));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error validating user {Username}", login.Username);
            return Result.Fail<User>(new Error("System error occurred")
                .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    public async Task<Result<User>> GetUserByIdAsync(long userId)
    {
        try
        {
            User? user = await this._userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return Result.Fail<User>(new Error($"User with ID {userId} not found")
                    .WithMetadata("errorType", ErrorType.NotFound));
            }

            return Result.Ok(user);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving user with ID {UserId}", userId);
            return Result.Fail<User>(new Error("Error retrieving user")
                .WithMetadata("errorType", ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    public async Task<Result<User>> GetUserByUsernameAsync(string username)
    {
        try
        {
            User? user = await this._userRepository.GetByUsernameAsync(username);

            if (user == null)
            {
                return Result.Fail<User>(new Error($"User with username {username} not found")
                    .WithMetadata("errorType", ErrorType.NotFound));
            }

            return Result.Ok(user);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving user with username {Username}", username);
            return Result.Fail<User>(new Error("Error retrieving user")
                .WithMetadata("errorType", ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    private static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
}
