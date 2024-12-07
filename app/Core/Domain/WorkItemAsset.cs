#region

using System.ComponentModel.DataAnnotations.Schema;

#endregion

namespace zora.Core.Domain;

[Table("zora_work_item_assets")]
public class WorkItemAsset : BaseCompositeEntity
{
    [Column("work_item_id")] public long WorkItemId { get; set; }

    [Column("asset_id")] public long AssetId { get; set; }

    [ForeignKey("WorkItemId")] public virtual WorkItem WorkItem { get; set; } = null!;

    [ForeignKey("AssetId")] public virtual Asset Asset { get; set; } = null!;
}
