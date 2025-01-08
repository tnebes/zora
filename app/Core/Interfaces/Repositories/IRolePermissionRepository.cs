#region

using FluentResults;
using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IRolePermissionRepository
{
    Task<Result<IEnumerable<RolePermission>>> GetByRoleIdAsync(long roleId, bool includeProperties = false);
    Task<Result<RolePermission>> CreateAsync(RolePermission rolePermission);
    Task<bool> DeleteByRoleId(long roleId);
    Task<Result<IEnumerable<RolePermission>>> CreateRangeAsync(List<RolePermission> rolePermissions);
}
