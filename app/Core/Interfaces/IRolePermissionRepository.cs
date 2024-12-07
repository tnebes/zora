using zora.Core.Domain;

namespace zora.Core.Interfaces;

public interface IRolePermissionRepository
{
    Task<IEnumerable<RolePermission>> GetByRoleIdAsync(long userRoleRoleId);
}
