namespace zora.Core.Domain;

public class WorkItemAsset : BaseCompositeEntity
{
    public long WorkItemId { get; set; }

    public long AssetId { get; set; }

    public virtual WorkItem WorkItem { get; set; } = null!;

    public virtual Asset Asset { get; set; } = null!;
}
