#region

using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("assets")]
public class Asset : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string AssetPath { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public long? CreatedById { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public long? UpdatedById { get; set; }

    public virtual User? CreatedBy { get; set; }

    public virtual User? UpdatedBy { get; set; }

    public virtual ICollection<WorkItemAsset> WorkItemAssets { get; set; } = new List<WorkItemAsset>();

    [NotMapped] public virtual IEnumerable<WorkItem> WorkItems => this.WorkItemAssets.Select(wia => wia.WorkItem);
}
