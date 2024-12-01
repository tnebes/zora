#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces;

public interface IUserRepository : IZoraService
{
    Task<User> GetUserByUsernameAsync(string username);
    Task<User> GetUserByEmailAsync(string email);
    Task<bool> ValidateUserCredentialsAsync(string username, string password);
    Task<User> GetUserByIdAsync(long id);
}
