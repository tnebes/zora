#region

using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.Enums;
using zora.Core.Interfaces;

#endregion

namespace zora.Infrastructure.Services;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class WorkItemService : IWorkItemService, IZoraService
{
    private readonly ILogger<WorkItemService> _logger;
    private readonly IWorkItemRepository _workItemRepository;

    public async Task<T?> GetNearestAncestorOf<T>(long resourceId) where T : WorkItem
    {
        try
        {
            WorkItem? workItem = await this._workItemRepository.GetWorkItemAsync(resourceId);
            if (workItem == null)
            {
                this._logger.LogWarning("Work item {ResourceId} not found", resourceId);
                return null;
            }

            WorkItem? ancestor = null;

            switch (workItem)
            {
                case ZoraTask task:
                    if (typeof(T) == typeof(Project))
                    {
                        ancestor = task.Project;
                    }
                    else if (typeof(T) == typeof(ZoraProgram))
                    {
                        ancestor = task.Project?.Program;
                    }

                    break;

                case Project project:
                    if (typeof(T) == typeof(ZoraProgram))
                    {
                        ancestor = project.Program;
                    }

                    break;

                case ZoraProgram:
                    this._logger.LogWarning("Program {ResourceId} has no ancestors", resourceId);
                    break;
            }

            return ancestor as T;
        }
        catch (Exception e)
        {
            this._logger.LogError(e, "Error getting nearest ancestor of work item {ResourceId}", resourceId);
            return null;
        }
    }

    public Task<WorkItemType> GetWorkItemType(long workItemId) =>
        this._workItemRepository.GetWorkItemTypeAsync(workItemId);
}
