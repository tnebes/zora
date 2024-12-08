#region

using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.Enums;
using zora.Core.Interfaces;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public class WorkItemRepository : BaseRepository<WorkItem>, IWorkItemRepository, IZoraService
{
    public WorkItemRepository(ApplicationDbContext dbContext, ILogger<WorkItemRepository> logger) : base(dbContext,
        logger)
    {
    }

    public Task<WorkItemType> GetWorkItemTypeAsync(long workItemId)
    {
        try
        {
            return this.DbSet
                .Where(wi => wi.Id == workItemId)
                .Select(wi => wi.Type)
                .Select(wit => Enum.Parse<WorkItemType>(wit))
                .FirstOrDefaultAsync();
        }
        catch (Exception e)
        {
            this.Logger.LogError(e, "Error getting work item type for work item {WorkItemId}", workItemId);
            return Task.FromResult(WorkItemType.NOT_DEFINED);
        }
    }

    public Task<WorkItem?> GetWorkItemAsync(long workItemId) => this.GetWorkItemByTypeAsync<WorkItem>(workItemId);

    public Task<T?> GetWorkItemByTypeAsync<T>(long workItemId) where T : WorkItem
    {
        return this.DbSet
            .OfType<T>()
            .Include(wi => wi.Assignee)
            .Include(wi => wi.CreatedBy)
            .Include(wi => wi.UpdatedBy)
            .Include(wi => wi.SourceRelationships)
            .Include(wi => wi.TargetRelationships)
            .Include(wi => wi.WorkItemAssets)
            .Include(wi => wi.PermissionWorkItems)
            .FirstOrDefaultAsync(wi => wi.Id == workItemId);
    }
}
