#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_work_item_relationships")]
public class WorkItemRelationship : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Column("source_item_id")] public long SourceItemId { get; set; }

    [Column("target_item_id")] public long TargetItemId { get; set; }

    [Required]
    [Column("relationship_type")]
    [StringLength(50)]
    public string RelationshipType { get; set; } = string.Empty;

    [Column("created_at")] public DateTime CreatedAt { get; set; }

    [ForeignKey("SourceItemId")] public virtual WorkItem SourceItem { get; set; } = null!;

    [ForeignKey("TargetItemId")] public virtual WorkItem TargetItem { get; set; } = null!;
}
