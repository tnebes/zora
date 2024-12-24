#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IRolePermissionRepository
{
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(long userRoleRoleId);
    Task CreateAsync(RolePermission rolePermission);
}
