#region

using zora.Core.Domain;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public sealed class RolePermissionRepository : BaseCompositeRepository<RolePermission>, IRolePermissionRepository,
    IZoraService
{
    public RolePermissionRepository(ApplicationDbContext dbContext, ILogger<RolePermissionRepository> logger) : base(
        dbContext, logger)
    {
    }

    public IQueryable<RolePermission> GetByRoleIdAsync(long userRoleRoleId)
    {
        IQueryable<RolePermission> rolePermissions = this
            .FindByCondition(rolePermission => rolePermission.RoleId == userRoleRoleId);
        return rolePermissions;
    }
}
