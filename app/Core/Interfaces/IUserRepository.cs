#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces;

public interface IUserRepository : IZoraService
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsUsernameUniqueAsync(string username);
    Task<bool> IsEmailUniqueAsync(string email);
}
