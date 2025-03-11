#region

using FluentResults;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.Enums;
using zora.Core.Interfaces.Repositories;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public sealed class WorkItemService : IWorkItemService, IZoraService
{
    private readonly ILogger<WorkItemService> _logger;
    private readonly IWorkItemRepository _workItemRepository;

    public WorkItemService(
        ILogger<WorkItemService> logger,
        IWorkItemRepository workItemRepository)
    {
        this._logger = logger;
        this._workItemRepository = workItemRepository;
    }

    public async Task<Result<T>> GetNearestAncestorOf<T>(long workItemId) where T : WorkItem
    {
        Result<WorkItem> workItemResult = await this.GetWorkItemWithValidation(workItemId);
        if (workItemResult.IsFailed)
        {
            return Result.Fail<T>(workItemResult.Errors);
        }

        return this.FindAncestor<T>(workItemResult.Value, workItemId);
    }

    public async Task<Result<WorkItemType>> GetWorkItemType(long workItemId)
    {
        Result<WorkItemType> result = await this._workItemRepository.GetWorkItemTypeAsync(workItemId);
        if (result.IsFailed)
        {
            this._logger.LogError("Failed to retrieve work item type for ID {WorkItemId}", workItemId);
            return Result.Fail<WorkItemType>($"Failed to retrieve work item type for ID {workItemId}");
        }

        return result.Value;
    }

    private async Task<Result<WorkItem>> GetWorkItemWithValidation(long workItemId)
    {
        try
        {
            Result<WorkItem> result = await this._workItemRepository.GetWorkItemAsync(workItemId, true);
            if (result.IsFailed)
            {
                this._logger.LogWarning("Work item {WorkItemId} not found", workItemId);
                return Result.Fail<WorkItem>($"Work item {workItemId} not found");
            }

            return result;
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error retrieving work item {WorkItemId}", workItemId);
            return Result.Fail<WorkItem>($"Error retrieving work item {workItemId}");
        }
    }

    private Result<T> FindAncestor<T>(WorkItem workItem, long workItemId) where T : WorkItem
    {
        try
        {
            T? ancestor = this.GetAncestorByType<T>(workItem);

            return ancestor != null
                ? Result.Ok(ancestor)
                : Result.Fail<T>($"Ancestor not found for work item {workItemId}");
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error finding ancestor of work item {WorkItemId}", workItemId);
            return Result.Fail<T>($"Error finding ancestor of work item {workItemId}");
        }
    }

    private T? GetAncestorByType<T>(WorkItem workItem) where T : WorkItem
    {
        return workItem switch
        {
            ZoraTask task when typeof(T) == typeof(Project) => task.Project as T,
            ZoraTask task when typeof(T) == typeof(ZoraProgram) => task.Project?.Program as T,
            Project project when typeof(T) == typeof(ZoraProgram) => project.Program as T,
            ZoraProgram => null,
            _ => null
        };
    }
}
