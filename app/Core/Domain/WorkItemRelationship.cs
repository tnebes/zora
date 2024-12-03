namespace zora.Core.Domain;

public class WorkItemRelationship : BaseEntity
{
    public long Id { get; set; }
    public long SourceItemId { get; set; }
    public WorkItem SourceItem { get; set; } = null!;
    public long TargetItemId { get; set; }
    public WorkItem TargetItem { get; set; } = null!;
    public string RelationshipType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
