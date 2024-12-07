#region

using zora.Core.DTOs;

#endregion

namespace zora.Core.Interfaces;

public interface IUserService : IZoraService
{
    Task<bool> ValidateUser(LoginRequestDto login);
}
