#region

using System.Security.Claims;
using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IUserService : IBaseService<User, CreateMinimumUserDto, UpdateUserDto, UserResponseDto<FullUserDto>,
    DynamicQueryUserParamsDto>
{
    Task<Result<User>> GetUserByUsernameAsync(string username);
    Task<Result<User>> ValidateUser(LoginRequestDto login);
    bool ClaimIsUser(ClaimsPrincipal httpContextUser, string username);
    T ToDto<T>(User user) where T : UserDto;
}
