using zora.Controllers;
using zora.Core.DTOs;

namespace zora.Core.Interfaces;

public interface IUserService : IZoraService
{
    Task<bool> ValidateUser(LoginRequest login);
}
