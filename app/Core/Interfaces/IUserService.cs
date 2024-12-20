#region

using System.Security.Claims;
using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.Core.Interfaces;

public interface IUserService
{
    Task<Result<User>> GetUserByIdAsync(long userId);
    Task<Result<User>> GetUserByUsernameAsync(string username);
    Task<Result<User>> ValidateUser(LoginRequestDto login);
    Task<IEnumerable<User>> GetUsersAsync(QueryParamsDto queryParams);
    Task<UserResponseDto<FullUserDto>> GetUsersDtoAsync(QueryParamsDto queryParams);
    bool ClaimIsUser(ClaimsPrincipal httpContextUser, string username);
    Task DeleteUserAsync(User user);
}
