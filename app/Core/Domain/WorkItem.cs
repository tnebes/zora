#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_work_items")]
public class WorkItem : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("type")]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("description")] public string? Description { get; set; }

    [Column("assignee_id")] public long? AssigneeId { get; set; }

    [Required]
    [Column("status")]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [Column("start_date")] public DateTime? StartDate { get; set; }

    [Column("due_date")] public DateTime? DueDate { get; set; }

    [Column("completion_percentage")] public decimal? CompletionPercentage { get; set; }

    [Column("estimated_hours")] public decimal? EstimatedHours { get; set; }

    [Column("actual_hours")] public decimal? ActualHours { get; set; }

    [Column("created_at")] public DateTime CreatedAt { get; set; }

    [Column("created_by")] public long? CreatedById { get; set; }

    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }

    [Column("updated_by")] public long? UpdatedById { get; set; }

    [ForeignKey("AssigneeId")] public virtual User? Assignee { get; set; }

    [ForeignKey("CreatedById")] public virtual User? CreatedBy { get; set; }

    [ForeignKey("UpdatedById")] public virtual User? UpdatedBy { get; set; }

    public virtual ZoraProgram? Program { get; set; }

    public virtual Project? Project { get; set; }

    public virtual ZoraTask? Task { get; set; }

    [InverseProperty("SourceItem")]
    public virtual ICollection<WorkItemRelationship> SourceRelationships { get; set; } =
        new List<WorkItemRelationship>();

    [InverseProperty("TargetItem")]
    public virtual ICollection<WorkItemRelationship> TargetRelationships { get; set; } =
        new List<WorkItemRelationship>();

    public virtual ICollection<WorkItemAsset> WorkItemAssets { get; set; } = new List<WorkItemAsset>();

    public virtual ICollection<PermissionWorkItem> PermissionWorkItems { get; set; } = new List<PermissionWorkItem>();

    [NotMapped] public virtual IEnumerable<Asset> Assets => this.WorkItemAssets.Select(wia => wia.Asset);
}
