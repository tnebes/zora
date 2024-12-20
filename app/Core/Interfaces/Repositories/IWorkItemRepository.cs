#region

using zora.Core.Domain;
using zora.Core.Enums;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IWorkItemRepository
{
    Task<WorkItemType> GetWorkItemTypeAsync(long workItemId);
    Task<WorkItem?> GetWorkItemAsync(long workItemId);
    Task<T?> GetWorkItemByTypeAsync<T>(long workItemId) where T : WorkItem;
}
