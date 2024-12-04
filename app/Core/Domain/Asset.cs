#region

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("assets")]
public class Asset : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("description")] public string? Description { get; set; }

    [Required] [Column("asset_path")] public string AssetPath { get; set; } = string.Empty;

    [Column("created_at")] public DateTime CreatedAt { get; set; }

    [Column("created_by")] public long? CreatedById { get; set; }

    [Column("updated_at")] public DateTime? UpdatedAt { get; set; }

    [Column("updated_by")] public long? UpdatedById { get; set; }

    [ForeignKey("CreatedById")] public virtual User? CreatedBy { get; set; }

    [ForeignKey("UpdatedById")] public virtual User? UpdatedBy { get; set; }

    public virtual ICollection<WorkItemAsset> WorkItemAssets { get; set; } = new List<WorkItemAsset>();

    [NotMapped] public virtual IEnumerable<WorkItem> WorkItems => this.WorkItemAssets.Select(wia => wia.WorkItem);
}
