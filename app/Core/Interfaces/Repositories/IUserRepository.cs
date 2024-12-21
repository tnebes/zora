#region

using zora.Core.Domain;
using zora.Core.DTOs.Requests;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByUsernameAsync(string username);
    Task<(IEnumerable<User>, int totalCount)> GetUsersAsync(QueryParamsDto queryParams);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> IsUsernameUniqueAsync(string username);
    Task<bool> IsEmailUniqueAsync(string email);
    void Delete(User user);
    Task SaveChangesAsync();
    void SoftDelete(User user);
    Task<User> Add(User user);
}
