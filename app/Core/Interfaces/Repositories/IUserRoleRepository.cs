#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IUserRoleRepository
{
    Task<IEnumerable<UserRole>> GetByUserIdAsync(long userId);
    Task<UserRole?> GetByCompositeKeyAsync(long userId, long roleId);
    Task<bool> ExistsAsync(long userId, long roleId);
    Task<bool> CreateAsync(UserRole entity);
    Task<bool> DeleteAsync(UserRole entity);
    Task<bool> AssignRoles(User user, List<long> roles);
    Task SaveChangesAsync();
}
