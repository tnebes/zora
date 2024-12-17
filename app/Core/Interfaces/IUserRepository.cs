#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetAllUsersAsync();
    Task<(IEnumerable<User>, int totalCount)> GetUsersAsync(int page, int pageSize);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsUsernameUniqueAsync(string username);
    Task<bool> IsEmailUniqueAsync(string email);
}
