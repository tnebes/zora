#region

using zora.Core.Domain;
using zora.Core.Enums;
using zora.Core.Interfaces;

#endregion

namespace zora.Infrastructure.Services;

public class WorkItemService : IWorkItemService, IZoraService
{
    public Task<WorkItem?> GetNearestAncestorOf(WorkItemType type, long permissionRequestResourceId) =>
        throw new NotImplementedException();

    public Task<WorkItemType> GetWorkItemType(long workItemId) => throw new NotImplementedException();
}
