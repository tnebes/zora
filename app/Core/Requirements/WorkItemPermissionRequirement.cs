#region

using Microsoft.AspNetCore.Authorization;
using zora.Core.Enums;

#endregion

namespace zora.Core.Requirements;

public sealed class WorkItemPermissionRequirement : IAuthorizationRequirement
{
    public WorkItemPermissionRequirement(long workItemId, PermissionFlag requiredPermission)
    {
        this.WorkItemId = workItemId;
        this.RequiredPermission = requiredPermission;
    }

    public long WorkItemId { get; }
    public PermissionFlag RequiredPermission { get; }
}
