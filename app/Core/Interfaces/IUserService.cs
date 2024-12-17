#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.DTOs;

#endregion

namespace zora.Core.Interfaces;

public interface IUserService
{
    Task<Result<User>> GetUserByIdAsync(long userId);
    Task<Result<User>> GetUserByUsernameAsync(string username);
    Task<Result<User>> ValidateUser(LoginRequestDto login);
    Task<UserResponseDto<FullUserDto>> GetFullUsersAsync(int page = 1, int pageSize = 50);
    Task<IEnumerable<User>> GetAllFullUsers();
}
