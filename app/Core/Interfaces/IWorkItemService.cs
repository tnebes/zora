using zora.Core.Domain;
using zora.Core.Enums;

namespace zora.Core.Interfaces;

public interface IWorkItemService
{
    Task<WorkItem?> GetNearestAncestorOf(WorkItemType type, long permissionRequestResourceId);

    Task<WorkItemType> GetWorkItemType(long workItemId);
}
