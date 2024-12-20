#region

using System.Security.Claims;
using AutoMapper;
using FluentResults;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Enums;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Helpers;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class UserService : IUserService, IZoraService
{
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper;
    private readonly IRoleService _roleService;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, IRoleService roleService, IMapper mapper,
        ILogger<UserService> logger)
    {
        this._userRepository = userRepository;
        this._roleService = roleService;
        this._mapper = mapper;
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

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(login.Password, user.Value.Password);

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

    public async Task<IEnumerable<User>> GetUsersAsync(QueryParamsDto queryParams)
    {
        (IEnumerable<User> users, int _) = await this._userRepository.GetUsersAsync(queryParams);
        return users;
    }

    public async Task<UserResponseDto<FullUserDto>> GetUsersDtoAsync(QueryParamsDto queryParams)
    {
        (IEnumerable<User> users, int totalCount) = await this._userRepository.GetUsersAsync(queryParams);
        UserResponseDto<FullUserDto> response =
            users.ToFullUserResponseDto(totalCount, queryParams.Page, queryParams.PageSize, this._mapper);
        return response;
    }

    public bool ClaimIsUser(ClaimsPrincipal httpContextUser, string username) =>
        httpContextUser.Identity?.Name == username;

    public Task DeleteUserAsync(User user)
    {
        user.Deleted = true;
        this._userRepository.SoftDelete(user);
        return this._userRepository.SaveChangesAsync();
    }

    public async Task<Result<User>> CreateAsync(CreateMinimumUserDto createMinimumUserDto)
    {
        try
        {
            User user = this._mapper.Map<User>(createMinimumUserDto);

            if (!this.IsValid(user))
            {
                return Result.Fail<User>(new Error("Invalid user data")
                    .WithMetadata("errorType", ErrorType.ValidationError));
            }

            user.Password = UserService.HashPassword(createMinimumUserDto.Password);
            await this._userRepository.Add(user);

            if (createMinimumUserDto.RoleIds.Any())
            {
                await this._roleService.AssignRoles(user, createMinimumUserDto.RoleIds);
            }

            return Result.Ok(user); // TODO return the new user
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating user {Username}", createMinimumUserDto.Username);
            return Result.Fail<User>(new Error("Error creating user")
                .WithMetadata("errorType", ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    public async Task<FullUserDto> GetUserDtoByIdAsync(long id) =>
        this._mapper.Map<FullUserDto>((await this.GetUserByIdAsync(id)).Value);

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

    private bool IsValid(User user)
    {
        return !string.IsNullOrWhiteSpace(user.Username) && !string.IsNullOrWhiteSpace(user.Email) &&
               !string.IsNullOrWhiteSpace(user.Password);
    }

    private static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
}
