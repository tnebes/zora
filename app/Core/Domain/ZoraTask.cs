#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_tasks")]
public class ZoraTask
{
    [Key] [Column("work_item_id")] public long WorkItemId { get; set; }

    [Column("project_id")] public long? ProjectId { get; set; }

    [Column("priority")]
    [StringLength(50)]
    public string? Priority { get; set; }

    [Column("parent_task_id")] public long? ParentTaskId { get; set; }

    [ForeignKey("WorkItemId")] public virtual WorkItem WorkItem { get; set; } = null!;

    [ForeignKey("ProjectId")] public virtual Project? Project { get; set; }

    [ForeignKey("ParentTaskId")] public virtual ZoraTask? ParentTask { get; set; }

    [InverseProperty("ParentTask")] public virtual ICollection<ZoraTask> SubTasks { get; set; } = new List<ZoraTask>();
}
