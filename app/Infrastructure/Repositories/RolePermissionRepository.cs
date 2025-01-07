#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using zora.Core;
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

    public async Task<Result<IEnumerable<RolePermission>>> GetByRoleIdAsync(long roleId)
    {
        try
        {
            List<RolePermission> rolePermissions = await this.DbContext.RolePermissions
                .Include(rp => rp.Permission)
                .Include(rp => rp.Role)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();

            return Result.Ok(rolePermissions.AsEnumerable());
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error retrieving role permissions for role {RoleId}", roleId);
            return Result.Fail("Error retrieving role permissions");
        }
    }

    public new async Task<Result<RolePermission>> CreateAsync(RolePermission rolePermission)
    {
        RolePermission createdRolePermission = await base.CreateAsync(rolePermission);
        if (createdRolePermission == null)
        {
            this.Logger.LogError("Error creating role permission {CreatedRolePermission}", createdRolePermission);
            return Result.Fail("Error creating role permission");
        }

        return Result.Ok(createdRolePermission);
    }

    public async Task<bool> DeleteByRoleId(long roleId)
    {
        IExecutionStrategy strategy = this.DbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using IDbContextTransaction transaction = await this.DbContext.Database.BeginTransactionAsync();
            try
            {
                IQueryable<RolePermission> rolePermissions = this
                    .FindByCondition(rolePermission => rolePermission.RoleId == roleId);

                if (!rolePermissions.Any())
                {
                    return false;
                }

                this.DbContext.RolePermissions.RemoveRange(rolePermissions);
                await this.DbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                this.Logger.LogError(ex, "Error deleting role permissions for role {RoleId}", roleId);
                return false;
            }
        });
    }

    public async Task<Result<IEnumerable<RolePermission>>> CreateRangeAsync(List<RolePermission> rolePermissions)
    {
        if (rolePermissions == null || !rolePermissions.Any())
        {
            return Result.Fail<IEnumerable<RolePermission>>("No role permissions to create.");
        }
        try
        {
            IExecutionStrategy strategy = this.DbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction transaction = await this.DbContext.Database.BeginTransactionAsync();
                try
                {
                    await this.DbContext.RolePermissions.AddRangeAsync(rolePermissions);
                    await this.DbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Result.Ok(rolePermissions.AsEnumerable());
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error creating range of role permissions: {Message}", ex.Message);
            return Result.Fail<IEnumerable<RolePermission>>(Constants.ERROR_500_MESSAGE);
        }
    }
}
