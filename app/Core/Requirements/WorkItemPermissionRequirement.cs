using Microsoft.AspNetCore.Authorization;
using zora.Core.Enums;

namespace zora.Core.Requirements;

public sealed class WorkItemPermissionRequirement : IAuthorizationRequirement
{
    public long WorkItemId { get; }
    public PermissionFlag RequiredPermission { get; }

    public WorkItemPermissionRequirement(long workItemId, PermissionFlag requiredPermission)
    {
        this.WorkItemId = workItemId;
        this.RequiredPermission = requiredPermission;
    }
}
