namespace zora.Core.Domain;

public class PermissionWorkItem : BaseCompositeEntity
{
    public long PermissionId { get; set; }

    public long WorkItemId { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual WorkItem WorkItem { get; set; } = null!;
}
