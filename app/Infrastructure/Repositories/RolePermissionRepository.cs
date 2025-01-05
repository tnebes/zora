#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
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

    public async Task<Result<IEnumerable<RolePermission>>> GetByRoleIdAsync(long userRoleRoleId)
    {
        IQueryable<RolePermission> rolePermissions = this
            .FindByCondition(rolePermission => rolePermission.RoleId == userRoleRoleId);
        return await rolePermissions.ToListAsync();
    }

    public new async Task<Result<RolePermission>> CreateAsync(RolePermission rolePermission)
    {
        RolePermission createdRolePermission = await base.CreateAsync(rolePermission);
        if (createdRolePermission == null)
        {
            this.Logger.LogError("Error creating role permission {CreatedRolePermission}", createdRolePermission);
            return Result.Fail("Error creating role permission");
        }
        else
        {
            return Result.Ok(createdRolePermission);
        }
    }

    public async Task<bool> DeleteByRoleId(long roleId)
    {
        IQueryable<RolePermission> rolePermissions = this
            .FindByCondition(rolePermission => rolePermission.RoleId == roleId);
        try
        {
            this.DbContext.RolePermissions.RemoveRange(rolePermissions);
            await this.DbContext.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error deleting role permissions for role {RoleId}", roleId);
            return false;
        }
    }
}
