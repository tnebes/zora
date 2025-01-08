#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.Enums;

#endregion

namespace zora.Core.Interfaces.Repositories;

public interface IWorkItemRepository
{
    Task<Result<WorkItemType>> GetWorkItemTypeAsync(long workItemId, bool includeProperties = false);
    Task<Result<WorkItem>> GetWorkItemAsync(long workItemId, bool includeProperties = false);
    Task<Result<T>> GetWorkItemByTypeAsync<T>(long workItemId) where T : WorkItem;
}
