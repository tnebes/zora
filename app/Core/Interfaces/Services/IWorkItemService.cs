#region

using zora.Core.Domain;
using zora.Core.Enums;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IWorkItemService
{
    Task<T?> GetNearestAncestorOf<T>(long resourceId) where T : WorkItem;

    Task<WorkItemType> GetWorkItemType(long workItemId);
}
