#region

using FluentResults;
using zora.Core.Domain;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IRolePermissionRepository
{
    Task<Result<IEnumerable<RolePermission>>> GetByRoleIdAsync(long userRoleRoleId);
    Task<Result<RolePermission>> CreateAsync(RolePermission rolePermission);
    Task<bool> DeleteByRoleId(long roleId);
}
