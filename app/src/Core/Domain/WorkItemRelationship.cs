namespace zora.Core.Domain;

public class WorkItemRelationship : BaseEntity
{
    public long SourceItemId { get; set; }

    public long TargetItemId { get; set; }

    public string RelationshipType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public virtual WorkItem SourceItem { get; set; } = null!;

    public virtual WorkItem TargetItem { get; set; } = null!;
}
