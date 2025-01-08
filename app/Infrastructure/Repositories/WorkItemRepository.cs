#region

using FluentResults;
using Microsoft.EntityFrameworkCore;
using zora.Core.Domain;
using zora.Core.Enums;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.Repositories;

public sealed class WorkItemRepository : BaseRepository<WorkItem>, IWorkItemRepository, IZoraService
{
    private readonly ILogger<WorkItemRepository> _logger;

    public WorkItemRepository(ApplicationDbContext dbContext, ILogger<WorkItemRepository> logger) : base(dbContext,
        logger) => this._logger = logger;

    public async Task<Result<WorkItemType>> GetWorkItemTypeAsync(long workItemId, bool includeProperties = false)
    {
        try
        {
            IQueryable<WorkItem> query = this.DbSet.AsQueryable();
            if (includeProperties)
            {
                query = this.IncludeProperties(query);
            }

            WorkItemType? workItemType = await query
                .Where(wi => wi.Id == workItemId)
                .Select(wi => wi.Type)
                .Select(wit => (WorkItemType?)Enum.Parse<WorkItemType>(wit))
                .FirstOrDefaultAsync();

            return workItemType == null
                ? Result.Fail<WorkItemType>($"Work item type not found for ID {workItemId}")
                : Result.Ok(workItemType.Value);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting work item type for work item {WorkItemId}", workItemId);
            return Result.Fail<WorkItemType>("Failed to retrieve work item type");
        }
    }

    public async Task<Result<WorkItem>> GetWorkItemAsync(long workItemId, bool includeProperties = false) =>
        await this.GetWorkItemByTypeAsync<WorkItem>(workItemId);

    public async Task<Result<T>> GetWorkItemByTypeAsync<T>(long workItemId) where T : WorkItem
    {
        try
        {
            IQueryable<WorkItem> query = this.DbSet.AsQueryable();
            query = this.IncludeProperties(query);

            T? workItem = await query
                .OfType<T>()
                .FirstOrDefaultAsync(wi => wi.Id == workItemId);

            return workItem == null
                ? Result.Fail<T>($"Work item of type {typeof(T).Name} not found for ID {workItemId}")
                : Result.Ok(workItem);
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error getting work item by type {WorkItemType} for ID {WorkItemId}",
                typeof(T).Name, workItemId);
            return Result.Fail<T>("Failed to retrieve work item by type");
        }
    }

    private IQueryable<WorkItem> IncludeProperties(IQueryable<WorkItem> query)
    {
        return query
            .Include(wi => wi.Assignee)
            .Include(wi => wi.CreatedBy)
            .Include(wi => wi.UpdatedBy)
            .Include(wi => wi.SourceRelationships)
            .Include(wi => wi.TargetRelationships)
            .Include(wi => wi.WorkItemAssets)
            .Include(wi => wi.PermissionWorkItems);
    }
}
