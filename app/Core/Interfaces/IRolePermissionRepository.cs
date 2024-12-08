#region

using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces;

public interface IRolePermissionRepository
{
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(long userRoleRoleId);
}
