#region

using zora.Core.DTOs;

#endregion

namespace zora.Core.Interfaces;

public interface IUserService
{
    Task<bool> ValidateUser(LoginRequestDto login);
}
