#region

using System.Security.Claims;
using AutoMapper;
using FluentResults;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.DTOs;
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
    private readonly IQueryService _queryService;
    private readonly IRoleService _roleService;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, IRoleService roleService, IQueryService queryService,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        this._userRepository = userRepository;
        this._roleService = roleService;
        this._queryService = queryService;
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

    public async Task<Result<(IEnumerable<User>, int total)>> GetAsync(QueryParamsDto queryParams)
    {
        try
        {
            (IEnumerable<User> users, int total) = await this._userRepository.GetUsersAsync(queryParams);
            return Result.Ok((users, total));
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting users");
            return Result.Fail<(IEnumerable<User>, int total)>(new Error("Error getting users")
                .WithMetadata(Constants.ERROR_TYPE, ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    public async Task<Result<UserResponseDto<FullUserDto>>> GetDtoAsync(QueryParamsDto queryParams)
    {
        try
        {
            (IEnumerable<User> users, int totalCount) = await this._userRepository.GetUsersAsync(queryParams);
            UserResponseDto<FullUserDto> response =
                users.ToFullUserResponseDto(totalCount, queryParams.Page, queryParams.PageSize, this._mapper);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting users");
            return Result.Fail<UserResponseDto<FullUserDto>>(new Error("Error getting users")
                .WithMetadata(Constants.ERROR_TYPE, ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    public async Task<Result<User>> GetByIdAsync(long id, bool includeProperties = false)
    {
        try
        {
            Result<User> userResult = await this._userRepository.GetByIdAsync(id, includeProperties);

            if (userResult.IsFailed)
            {
                this._logger.LogWarning("User with ID {UserId} not found", id);
                return Result.Fail<User>(new Error($"User with ID {id} not found")
                    .WithMetadata("errorType", ErrorType.NotFound));
            }

            return userResult;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving user with ID {UserId}", id);
            return Result.Fail<User>(new Error("Error retrieving user")
                .WithMetadata("errorType", ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    public async Task<Result<User>> CreateAsync(CreateMinimumUserDto createDto)
    {
        try
        {
            User user = this._mapper.Map<User>(createDto);

            if (!IsValid(user))
            {
                return Result.Fail<User>(new Error("Invalid user data")
                    .WithMetadata("errorType", ErrorType.ValidationError));
            }

            user.Password = HashPassword(createDto.Password);
            await this._userRepository.Add(user);

            if (createDto.RoleIds.Any())
            {
                await this._roleService.AssignRoles(user, createDto.RoleIds);
            }

            return Result.Ok(user);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error creating user {Username}", createDto.Username);
            return Result.Fail<User>(new Error("Error creating user")
                .WithMetadata("errorType", ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    public async Task<Result<User>> UpdateAsync(long id, UpdateUserDto updateDto)
    {
        Result<User> originalUserResult = await this._userRepository.GetByIdAsync(id);

        if (originalUserResult.IsFailed)
        {
            this._logger.LogWarning("User with ID {UserId} not found", id);
            return Result.Fail<User>(new Error("User not found")
                .WithMetadata("errorType", ErrorType.NotFound));
        }

        User originalUser = originalUserResult.Value;
        originalUser.Email = updateDto.Email;
        originalUser.Username = updateDto.Username;

        HashSet<long> existingRoleIds = originalUser.UserRoles.Select(ur => ur.RoleId).ToHashSet();
        HashSet<long> newRoleIds = updateDto.RoleIds.ToHashSet();

        List<UserRole> rolesToRemove = originalUser.UserRoles.Where(ur => !newRoleIds.Contains(ur.RoleId)).ToList();
        foreach (UserRole roleToRemove in rolesToRemove)
        {
            originalUser.UserRoles.Remove(roleToRemove);
        }

        IEnumerable<long> rolesToAdd = newRoleIds.Where(roleId => !existingRoleIds.Contains(roleId));
        foreach (long roleId in rolesToAdd)
        {
            UserRole userRole = new UserRole
            {
                UserId = originalUser.Id,
                RoleId = roleId,
                Role = (await this._roleService.GetByIdAsync(roleId)).Value
            };
            originalUser.UserRoles.Add(userRole);
        }

        Result<User> updatedUser = await this._userRepository.Update(originalUser);
        return updatedUser;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            Result<User> userResult = await this.GetByIdAsync(id);
            if (userResult.IsFailed)
            {
                return false;
            }

            User user = userResult.Value;
            user.Deleted = true;
            await this._userRepository.SoftDelete(user);
            return true;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error deleting user with ID {UserId}", id);
            return false;
        }
    }

    public async Task<Result<UserResponseDto<FullUserDto>>> FindAsync(QueryParamsDto findParams)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(findParams.SearchTerm) || findParams.SearchTerm.Length < 3)
            {
                return Result.Fail<UserResponseDto<FullUserDto>>("Search term must be at least 3 characters long");
            }

            Result<(IEnumerable<User>, int totalCount)> result = await this._userRepository.FindUsersAsync(findParams);

            if (result.IsFailed)
            {
                return Result.Fail<UserResponseDto<FullUserDto>>("Error finding users");
            }

            (IEnumerable<User> users, int totalCount) = result.Value;
            UserResponseDto<FullUserDto> response =
                users.ToFullUserResponseDto(totalCount, findParams.Page, findParams.PageSize, this._mapper);

            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error finding users with term {SearchTerm}", findParams.SearchTerm);
            return Result.Fail<UserResponseDto<FullUserDto>>("Error finding users");
        }
    }

    public async Task<Result<UserResponseDto<FullUserDto>>> SearchAsync(DynamicQueryUserParamsDto searchParams)
    {
        try
        {
            this._queryService.ValidateQueryParams(searchParams, ResourceType.User);
            Result<(IEnumerable<User> users, int TotalCount)> searchUsers =
                await this._userRepository.SearchAsync(searchParams);

            if (searchUsers.IsFailed)
            {
                return Result.Fail<UserResponseDto<FullUserDto>>(new Error("Error searching users")
                    .WithMetadata(Constants.ERROR_TYPE, ErrorType.SystemError));
            }

            (IEnumerable<User> users, int totalCount) = searchUsers.Value;

            UserResponseDto<FullUserDto> response =
                users.ToFullUserResponseDto(totalCount, searchParams.Page, searchParams.PageSize, this._mapper);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error searching users");
            return Result.Fail<UserResponseDto<FullUserDto>>(new Error("Error searching users")
                .WithMetadata(Constants.ERROR_TYPE, ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    public bool ClaimIsUser(ClaimsPrincipal httpContextUser, string username) =>
        httpContextUser.Identity?.Name == username;

    public async Task<Result<User>> GetUserByUsernameAsync(string username)
    {
        try
        {
            Result<User> userResult = await this._userRepository.GetByUsernameAsync(username);

            if (userResult.IsFailed)
            {
                this._logger.LogWarning("User with username {Username} not found", username);
                return Result.Fail<User>(new Error($"User with username {username} not found")
                    .WithMetadata("errorType", ErrorType.NotFound));
            }

            return userResult;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving user with username {Username}", username);
            return Result.Fail<User>(new Error("Error retrieving user")
                .WithMetadata("errorType", ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    public T ToDto<T>(User user) where T : UserDto => this._mapper.Map<T>(user);

    public async Task<Result<User>> GetUserByIdAsync(long userId)
    {
        try
        {
            Result<User> userResult = await this._userRepository.GetByIdAsync(userId);

            if (userResult.IsFailed)
            {
                this._logger.LogWarning("User with ID {UserId} not found", userId);
                return Result.Fail<User>(new Error($"User with ID {userId} not found")
                    .WithMetadata("errorType", ErrorType.NotFound));
            }

            return userResult;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving user with ID {UserId}", userId);
            return Result.Fail<User>(new Error("Error retrieving user")
                .WithMetadata("errorType", ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    public async Task<Result<FullUserDto>> GetUserDtoByIdAsync(long id)
    {
        try
        {
            User updatedUserValue = (await this.GetUserByIdAsync(id)).Value;

            if (updatedUserValue == null)
            {
                return Result.Fail<FullUserDto>(new Error("User not found")
                    .WithMetadata("errorType", ErrorType.NotFound));
            }

            return this.ToDto<FullUserDto>(updatedUserValue);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting user with ID {UserId}", id);
            return Result.Fail<FullUserDto>(new Error("Error getting user")
                .WithMetadata("errorType", ErrorType.SystemError)
                .WithMetadata("exception", ex));
        }
    }

    private static bool IsValid(User user)
    {
        return !string.IsNullOrWhiteSpace(user.Username) && !string.IsNullOrWhiteSpace(user.Email) &&
               !string.IsNullOrWhiteSpace(user.Password);
    }

    private static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
}
