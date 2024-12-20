#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IUserRoleService
{
    IQueryable<UserRole> GetUserRolesByUserIdAsync(long userId);
    Task<bool> IsRoleAsync(IEnumerable<UserRole> userRoles, string roleName);
    Task<bool> IsRoleAsync(long userId, string roleName);
    Task<bool> IsAdminAsync(IEnumerable<UserRole> userRoles);
    Task<bool> IsAdminAsync(long userId);
}
