#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.Interfaces;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public class RolePermissionRepository : BaseCompositeRepository<RolePermission>, IRolePermissionRepository, IZoraService
{
    public RolePermissionRepository(ApplicationDbContext dbContext, ILogger<RolePermissionRepository> logger) : base(
        dbContext, logger)
    {
    }

    public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(long userRoleRoleId)
    {
        List<RolePermission> rolePermissions = await this
            .FindByCondition(rolePermission => rolePermission.RoleId == userRoleRoleId)
            .ToListAsync();
        return rolePermissions;
    }
}
