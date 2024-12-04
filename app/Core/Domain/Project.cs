#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_projects")]
public class Project
{
    [Key] [Column("work_item_id")] public long WorkItemId { get; set; }

    [Column("program_id")] public long? ProgramId { get; set; }

    [Column("project_manager_id")] public long? ProjectManagerId { get; set; }

    [ForeignKey("WorkItemId")] public virtual WorkItem WorkItem { get; set; } = null!;

    [ForeignKey("ProgramId")] public virtual ZoraProgram? Program { get; set; }

    [ForeignKey("ProjectManagerId")] public virtual User? ProjectManager { get; set; }

    [InverseProperty("Project")] public virtual ICollection<ZoraTask> Tasks { get; set; } = new List<ZoraTask>();
}
