#region

using FluentResults;
using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IUserRoleRepository
{
    Task<Result<IEnumerable<UserRole>>> GetByUserIdAsync(long userId, bool includeProperties = false);
    Task<Result<UserRole>> GetByCompositeKeyAsync(long userId, long roleId, bool includeProperties = false);
    Task<bool> ExistsAsync(long userId, long roleId);
    Task<Result<UserRole>> CreateAsync(UserRole entity);
    Task<bool> DeleteAsync(UserRole entity);
    Task<bool> AssignRoles(User user, List<long> roles);
}
