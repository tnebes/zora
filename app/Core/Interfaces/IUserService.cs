using zora.Controllers;
using zora.Core.DTOs;

namespace zora.Core.Interfaces;

public interface IUserService
{
    bool ValidateUser(LoginRequest login);
}
