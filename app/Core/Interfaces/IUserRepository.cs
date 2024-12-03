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
    Task<IEnumerable<User>> GetUsersWithRolesAsync();
    Task<bool> IsUsernameUniqueAsync(string username);
    Task<bool> IsEmailUniqueAsync(string email);
}
