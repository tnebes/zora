#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_programs")]
public class ZoraProgram
{
    [Key] [Column("work_item_id")] public long WorkItemId { get; set; }

    [Column("description")] public string? Description { get; set; }

    [ForeignKey("WorkItemId")] public virtual WorkItem WorkItem { get; set; } = null!;

    [InverseProperty("Program")] public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
