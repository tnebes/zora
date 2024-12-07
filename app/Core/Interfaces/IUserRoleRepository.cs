#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces;

public interface IUserRoleRepository
{
    Task<IEnumerable<UserRole>> GetByUserIdAsync(long userId);
    Task<UserRole?> GetByCompositeKeyAsync(long userId, long roleId);
    Task<bool> ExistsAsync(long userId, long roleId);
    Task<bool> CreateAsync(UserRole entity);
    Task<bool> DeleteAsync(UserRole entity);
}
