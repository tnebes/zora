#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public sealed class PermissionWorkItemRepository : BaseCompositeRepository<PermissionWorkItem>,
    IPermissionWorkItemRepository,
    IZoraService
{
    public PermissionWorkItemRepository(ApplicationDbContext dbContext, ILogger<PermissionWorkItemRepository> logger) :
        base(dbContext, logger)
    {
    }

    public async Task<Result<PermissionWorkItem>> GetByCompositeKeyAsync(long permissionId, long requestResourceId,
        bool includeProperties = false)
    {
        try
        {
            IQueryable<PermissionWorkItem> query = this.DbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            PermissionWorkItem? permissionWorkItem = await query
                .Where(pwi => pwi.PermissionId == permissionId && pwi.WorkItemId == requestResourceId)
                .FirstOrDefaultAsync();

            return permissionWorkItem == null
                ? Result.Fail<PermissionWorkItem>("Permission work item not found")
                : Result.Ok(permissionWorkItem);
        }
        catch (Exception e)
        {
            this.Logger.LogError(e, "Error while getting permission work item by composite key");
            return Result.Fail<PermissionWorkItem>("Error while getting permission work item by composite key");
        }
    }

    private IQueryable<PermissionWorkItem> IncludeProperties(IQueryable<PermissionWorkItem> query)
    {
        return query.Include(pwi => pwi.Permission)
            .ThenInclude(p => p.RolePermissions)
            .ThenInclude(rp => rp.Role);
    }
}
