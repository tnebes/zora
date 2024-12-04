#region

using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_permission_work_items")]
public class PermissionWorkItem : BaseEntity
{
    [Column("permission_id")] public long PermissionId { get; set; }

    [Column("work_item_id")] public long WorkItemId { get; set; }

    [ForeignKey("PermissionId")] public virtual Permission Permission { get; set; } = null!;

    [ForeignKey("WorkItemId")] public virtual WorkItem WorkItem { get; set; } = null!;
}
