#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IRolePermissionRepository
{
    IQueryable<RolePermission> GetByRoleIdAsync(long userRoleRoleId);
}
