#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.Interfaces;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public class PermissionWorkItemRepository : BaseCompositeRepository<PermissionWorkItem>, IPermissionWorkItemRepository,
    IZoraService
{
    public PermissionWorkItemRepository(ApplicationDbContext dbContext, ILogger<PermissionWorkItemRepository> logger) :
        base(dbContext, logger)
    {
    }

    public Task<PermissionWorkItem?> GetByCompositeKeyAsync(long permissionId, long requestResourceId)
    {
        return this.FindByCondition(permissionWorkItem =>
                permissionWorkItem.PermissionId == permissionId &&
                permissionWorkItem.WorkItemId == requestResourceId)
            .FirstOrDefaultAsync();
    }
}
