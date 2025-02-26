#region

using FluentResults;
using zora.Core.Domain;
using zora.Core.Enums;

#endregion

namespace zora.Core.Interfaces.Services;

public interface IWorkItemService
{
    Task<Result<T>> GetNearestAncestorOf<T>(long resourceId) where T : WorkItem;

    Task<Result<WorkItemType>> GetWorkItemType(long workItemId);
}
